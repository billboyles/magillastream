import subprocess
import os
from paths import ffmpeg_path

# Define a mapping of encoder shortcodes to user-friendly names and best containers
ENCODER_INFO = {
    'libx264': ('H.264 (libx264)', 'flv'),
    'h264_qsv': ('H.264 (Intel Quick Sync)', 'flv'),
    'libx265': ('HEVC (libx265)', 'mp4'),
    'hevc_qsv': ('HEVC (Intel Quick Sync)', 'mp4'),
    'libaom-av1': ('AV1 (libaom-av1)', 'mp4'),
    'h264_nvenc': ('H.264 (NVIDIA NVENC)', 'flv'),
    'hevc_nvenc': ('HEVC (NVIDIA NVENC)', 'mp4'),
    'h264_amf': ('H.264 (AMD AMF)', 'flv'),
    'hevc_amf': ('HEVC (AMD AMF)', 'mp4'),
}

def list_ffmpeg_encoders():
    try:
        # Build the FFmpeg executable path
        ffmpeg_executable = os.path.join(ffmpeg_path, 'ffmpeg.exe')
        
        # Check if the FFmpeg executable exists
        if not os.path.isfile(ffmpeg_executable):
            raise FileNotFoundError(f"FFmpeg executable not found at {ffmpeg_executable}")
        
        # Run FFmpeg command to list encoders
        result = subprocess.run([ffmpeg_executable, '-encoders'], capture_output=True, text=True, check=True)
        
        # Define the encoders of interest
        encoders_of_interest = list(ENCODER_INFO.keys())
        
        # Filter the result
        filtered_lines = [line for line in result.stdout.splitlines() if any(encoder in line for encoder in encoders_of_interest)]
        
        # Create a list of encoder shortcodes, their user-friendly names, and best containers
        encoders_list = []
        for line in filtered_lines:
            for encoder in encoders_of_interest:
                if encoder in line:
                    user_friendly_name, container = ENCODER_INFO.get(encoder, (encoder, 'unknown'))
                    encoders_list.append((encoder, user_friendly_name, container))
        
        return encoders_list

    except FileNotFoundError as e:
        print(f"FileNotFoundError occurred: {e}")
        return []
    except subprocess.CalledProcessError as e:
        print(f"Error occurred while running FFmpeg: {e}")
        return []
