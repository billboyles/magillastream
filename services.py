import subprocess
import os
import psutil
import logging
from paths import nginx_path, ffmpeg_path, nginx_conf_file, dynamic_rtmp_template_file, conf_dir

# Audio encoder mapping (human-readable names to FFmpeg-compatible codes)
AUDIO_ENCODER_MAP = {
    "AAC": "aac",
    "MP3": "libmp3lame",
    "Opus": "libopus"
}

def sanitize_url(url):
    url = url.strip()
    if url.endswith('/'):
        url = url[:-1]
    if '?' in url:
        url = url.split('?')[0]
    if url.startswith(('http://', 'https://', 'rtmp://')):
        return url
    return ""

def sanitize_numeric(value, default=0):
    try:
        return int(value)
    except ValueError:
        return default

def sanitize_stream_key(key):
    return key.strip()

def sanitize_resolution(resolution):
    resolution_map = {
        "360p": "640x360",
        "480p": "854x480",
        "720p": "1280x720",
        "1080p": "1920x1080",
        "1440p": "2560x1440",
        "4K": "3840x2160"
    }
    return resolution_map.get(resolution, "1920x1080")  # Default to 1080p if resolution is not found

def sanitize_bitrate(bitrate, default="6000k"):
    if bitrate.lower().endswith('k') and bitrate[:-1].isdigit():
        return bitrate.lower()
    return default

def sanitize_framerate(framerate):
    if framerate.isdigit():
        return framerate
    return "30"

def start_services(config):
    youtube_primary_ingest_url = sanitize_url(config.get("youtube_primary_ingest_url", ""))
    youtube_backup_ingest_url = sanitize_url(config.get("youtube_backup_ingest_url", ""))
    twitch_ingest_url = sanitize_url(config.get("twitch_ingest_url", ""))
    youtube_stream_key = sanitize_stream_key(config.get("youtube_stream_key", ""))
    twitch_key = sanitize_stream_key(config.get("twitch_key", ""))
    incoming_port = sanitize_numeric(config.get("incoming_port", 1935), default=1935)
    incoming_app = config.get("incoming_app", "").strip()

    # Video encoder and container from config
    encoder_code, encoder_name, container = config.get("ffmpeg_encoder", ("libx264", "H.264 (libx264)", "flv"))
    ffmpeg_encoder = encoder_code.strip()

    # Ensure the container format is valid (default to "flv")
    if container.lower() not in ["flv", "mp4", "mpegts"]:
        container = "flv"  # Default format for RTMP streaming

    # Get the correct resolution format (width x height)
    ffmpeg_resolution = sanitize_resolution(config.get("ffmpeg_resolution", "720p"))  # Default to 720p

    # Get other necessary configurations
    ffmpeg_bitrate = sanitize_bitrate(config.get("ffmpeg_bitrate", "6000k"))
    ffmpeg_framerate = sanitize_framerate(config.get("ffmpeg_framerate", "30"))
    ffmpeg_preset = config.get("ffmpeg_preset", "medium")  # Get the preset from the configuration

    # Audio encoding options with correct mapping
    audio_encoder = AUDIO_ENCODER_MAP.get(config.get("audio_encoder", "AAC"), "aac")
    audio_bitrate = sanitize_bitrate(config.get("audio_bitrate", "192k"), default="192k")

    # Calculate the buffer size as twice the video bitrate
    buffer_size = str(int(ffmpeg_bitrate[:-1]) * 2) + 'k'

    # Load the RTMP template
    template_path = dynamic_rtmp_template_file
    try:
        with open(template_path, 'r') as template_file:
            template = template_file.read()
    except Exception as e:
        logging.error(f"Failed to read template file: {e}")
        return

    # Fill in the dynamic RTMP configuration
    dynamic_config = template.format(
        incoming_port=incoming_port,
        incoming_app=incoming_app,
        youtube_primary_ingest_url=youtube_primary_ingest_url,
        youtube_backup_ingest_url=youtube_backup_ingest_url,
        youtube_stream_key=youtube_stream_key,
        twitch_ingest_url=twitch_ingest_url,
        twitch_key=twitch_key
    )

    # Write the dynamic RTMP configuration to the nginx conf file
    rtmp_conf_path = os.path.join(conf_dir, 'rtmp.conf')
    try:
        if os.path.exists(rtmp_conf_path):
            os.remove(rtmp_conf_path)
        with open(rtmp_conf_path, 'w') as dynamic_file:
            dynamic_file.write(dynamic_config)
    except Exception as e:
        logging.error(f"Failed to write RTMP config file: {e}")
        return

    # Remove old nginx.pid file if it exists
    nginx_pid_file = os.path.join(nginx_path, 'logs', 'nginx.pid')
    if os.path.exists(nginx_pid_file):
        try:
            os.remove(nginx_pid_file)
            logging.info("Removed old nginx.pid file")
        except Exception as e:
            logging.error(f"Failed to remove old nginx.pid file: {e}")
            return

    # Start Nginx
    nginx_executable = os.path.join(nginx_path, 'nginx.exe')
    if not os.path.exists(nginx_executable):
        logging.error(f"Error: {nginx_executable} not found")
        return
    logging.debug("Starting Nginx with executable: %s", nginx_executable)
    process = subprocess.Popen([nginx_executable, '-c', nginx_conf_file], cwd=nginx_path)
    logging.debug("Nginx started, process ID: %s", process.pid)

    # Start FFmpeg processes
    ffmpeg_executable = os.path.join(ffmpeg_path, 'ffmpeg.exe')
    if not os.path.exists(ffmpeg_executable):
        logging.error(f"Error: {ffmpeg_executable} not found")
        return
    ffmpeg_processes = []

    try:
        ffmpeg_processes.append(subprocess.Popen([
            ffmpeg_executable,
            '-re',
            '-i', f'rtmp://127.0.0.1:{incoming_port}/{incoming_app}',
            '-r', ffmpeg_framerate,
            '-s', ffmpeg_resolution,       # Pass the correct resolution to FFmpeg
            '-b:v', ffmpeg_bitrate,         # Target video bitrate
            '-maxrate', ffmpeg_bitrate,     # Ensure max bitrate matches target
            '-minrate', ffmpeg_bitrate,     # Ensure min bitrate matches target
            '-bufsize', buffer_size,        # Buffer size set to twice the bitrate
            '-c:v', ffmpeg_encoder,         # Video encoder
            '-preset', ffmpeg_preset,       # Use the preset from the configuration
            '-b:a', audio_bitrate,          # Target audio bitrate
            '-c:a', audio_encoder,          # Audio encoder
            '-f', container,                # Correct container format (e.g., flv)
            f'rtmp://127.0.0.1:{incoming_port}/reencoded_distribution'
        ], cwd=ffmpeg_path))
    except Exception as e:
        logging.error(f"Failed to start FFmpeg process: {e}")
        return

    return ffmpeg_processes

def stop_services(ffmpeg_processes):
    # Stop FFmpeg processes
    for process in ffmpeg_processes:
        if process.poll() is None:  # Check if the process is still running
            process.terminate()  # Send termination signal
            try:
                process.wait(timeout=10)  # Wait for the process to exit gracefully
            except subprocess.TimeoutExpired:
                process.kill()  # Force kill the process if it doesn't exit in time
                logging.warning(f"Force killed FFmpeg process (PID: {process.pid})")

    # Stop Nginx
    nginx_pid_file = os.path.join(nginx_path, 'logs', 'nginx.pid')
    if os.path.exists(nginx_pid_file):
        try:
            nginx_executable = os.path.join(nginx_path, 'nginx.exe')
            subprocess.run([nginx_executable, '-s', 'stop'], cwd=nginx_path, check=True)
        except subprocess.CalledProcessError:
            logging.error(f"Failed to stop {nginx_executable} gracefully")
            # If stopping nginx gracefully fails, force kill any lingering nginx processes
            for proc in psutil.process_iter(['pid', 'name']):
                if 'nginx' in proc.info['name'].lower():
                    logging.warning(f"Force killing nginx process: {proc.info['name']} (PID: {proc.info['pid']})")
                    proc.terminate()
                    proc.wait()
    else:
        logging.error(f"nginx.pid file not found: Unable to stop Nginx")
