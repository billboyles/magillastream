import subprocess
import os
import psutil
from paths import nginx_path, ffmpeg_path, nginx_conf_file, dynamic_rtmp_template_file, conf_dir

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
    return resolution_map.get(resolution, "1920x1080")  # Default to 1080p

def sanitize_bitrate(bitrate):
    if bitrate.lower().endswith('k') and bitrate[:-1].isdigit():
        return bitrate.lower()
    return "6000k"

def sanitize_framerate(framerate):
    if framerate.isdigit():
        return framerate
    return "30"

def start_services(config):
    youtube_primary_ingress_url = sanitize_url(config.get("youtube_primary_ingress_url", ""))
    youtube_backup_ingress_url = sanitize_url(config.get("youtube_backup_ingress_url", ""))
    twitch_ingress_url = sanitize_url(config.get("twitch_ingress_url", ""))
    youtube_stream_key = sanitize_stream_key(config.get("youtube_stream_key", ""))
    twitch_key = sanitize_stream_key(config.get("twitch_key", ""))
    incoming_port = sanitize_numeric(config.get("incoming_port", 1935), default=1935)
    incoming_app = config.get("incoming_app", "").strip()

    encoder_code, encoder_name, container = config.get("ffmpeg_encoder", ("libx264", "H.264 (libx264)", "flv"))
    ffmpeg_encoder = encoder_code.strip()
    ffmpeg_resolution = sanitize_resolution(config.get("ffmpeg_resolution", "720p"))  # Default to 720p
    ffmpeg_bitrate = sanitize_bitrate(config.get("ffmpeg_bitrate", "6000k"))
    ffmpeg_framerate = sanitize_framerate(config.get("ffmpeg_framerate", "30"))
    
    # Calculate the buffer size as twice the bitrate
    buffer_size = str(int(ffmpeg_bitrate[:-1]) * 2) + 'k'

    template_path = dynamic_rtmp_template_file
    with open(template_path, 'r') as template_file:
        template = template_file.read()

    dynamic_config = template.format(
        incoming_port=incoming_port,
        incoming_app=incoming_app,
        youtube_primary_ingress_url=youtube_primary_ingress_url,
        youtube_backup_ingress_url=youtube_backup_ingress_url,
        youtube_stream_key=youtube_stream_key,
        twitch_ingress_url=twitch_ingress_url,
        twitch_key=twitch_key
    )

    rtmp_conf_path = os.path.join(conf_dir, 'rtmp.conf')
    if os.path.exists(rtmp_conf_path):
        os.remove(rtmp_conf_path)

    with open(rtmp_conf_path, 'w') as dynamic_file:
        dynamic_file.write(dynamic_config)

    nginx_executable = os.path.join(nginx_path, 'nginx.exe')
    if not os.path.exists(nginx_executable):
        print(f"Error: {nginx_executable} not found")
        return
    subprocess.Popen([nginx_executable, '-c', nginx_conf_file], cwd=nginx_path)

    ffmpeg_executable = os.path.join(ffmpeg_path, 'ffmpeg.exe')
    if not os.path.exists(ffmpeg_executable):
        print(f"Error: {ffmpeg_executable} not found")
        return
    ffmpeg_processes = []

    ffmpeg_processes.append(subprocess.Popen([
        ffmpeg_executable,
        '-re',
        '-i', 'rtmp://127.0.0.1:1935/ffmpeg',
        '-r', ffmpeg_framerate,
        '-s', ffmpeg_resolution,
        '-b:v', ffmpeg_bitrate,         # Target video bitrate
        '-maxrate', ffmpeg_bitrate,     # Ensure max bitrate matches target
        '-minrate', ffmpeg_bitrate,     # Ensure min bitrate matches target
        '-bufsize', buffer_size,        # Buffer size set to twice the bitrate
        '-c:v', ffmpeg_encoder,         # Video encoder
        '-preset', 'veryfast',
        '-b:a', '192k',                 # Target audio bitrate (you can adjust this)
        '-c:a', 'aac',                  # Audio encoder
        '-f', container,
        'rtmp://127.0.0.1:1935/reencoded_distribution'
    ], cwd=ffmpeg_path))

    return ffmpeg_processes


def stop_services(ffmpeg_processes):
    # Stop FFmpeg processes
    for process in ffmpeg_processes:
        if process.poll() is None:
            process.terminate()
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                process.kill()

    # Stop Nginx
    nginx_executable = os.path.join(nginx_path, 'nginx.exe')
    if os.path.exists(nginx_executable):
        try:
            # Attempt to stop nginx gracefully
            subprocess.run([nginx_executable, '-s', 'stop'], cwd=nginx_path, check=True)
        except subprocess.CalledProcessError:
            print(f"Error: Failed to stop {nginx_executable}")
            # If stopping nginx gracefully fails, force kill all nginx processes
            for proc in psutil.process_iter(['pid', 'name']):
                if 'nginx' in proc.info['name'].lower():
                    print(f"Force killing nginx process: {proc.info['name']} (PID: {proc.info['pid']})")
                    proc.terminate()
                    proc.wait()

    # Ensure all lingering processes are stopped
    for proc in psutil.process_iter(['pid', 'name']):
        try:
            if 'nginx' in proc.info['name'].lower() or 'ffmpeg' in proc.info['name'].lower():
                print(f"Terminating lingering process: {proc.info['name']} (PID: {proc.info['pid']})")
                proc.terminate()
                proc.wait()
        except (psutil.NoSuchProcess, psutil.AccessDenied, psutil.ZombieProcess):
            pass
