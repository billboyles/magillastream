import subprocess
import os
from paths import ffmpeg_path

# Define a mapping of encoder shortcodes to user-friendly names and best containers
ENCODER_INFO = {
    'libx264': ('H.264 (libx264)', 'flv', ['ultrafast', 'superfast', 'veryfast', 'faster', 'fast', 'medium', 'slow', 'slower', 'veryslow']),
    'h264_qsv': ('H.264 (Intel Quick Sync)', 'flv', ['veryfast', 'fast', 'medium', 'slow']),
    'libx265': ('HEVC (libx265)', 'mp4', ['ultrafast', 'superfast', 'veryfast', 'faster', 'fast', 'medium', 'slow', 'slower', 'veryslow']),
    'hevc_qsv': ('HEVC (Intel Quick Sync)', 'mp4', ['veryfast', 'fast', 'medium', 'slow']),
    'libaom-av1': ('AV1 (libaom-av1)', 'mp4', ['realtime', 'good', 'best']),
    'h264_nvenc': ('H.264 (NVIDIA NVENC)', 'flv', ['p1', 'p2', 'p3', 'p4', 'p5', 'p6', 'p7']),
    'hevc_nvenc': ('HEVC (NVIDIA NVENC)', 'mp4', ['p1', 'p2', 'p3', 'p4', 'p5', 'p6', 'p7']),
    'h264_amf': ('H.264 (AMD AMF)', 'flv', ['balanced', 'quality', 'speed']),
    'hevc_amf': ('HEVC (AMD AMF)', 'mp4', ['balanced', 'quality', 'speed']),
}

def list_ffmpeg_encoders():
    try:
        ffmpeg_executable = os.path.join(ffmpeg_path, 'ffmpeg.exe')
        
        if not os.path.isfile(ffmpeg_executable):
            raise FileNotFoundError(f"FFmpeg executable not found at {ffmpeg_executable}")
        
        result = subprocess.run([ffmpeg_executable, '-encoders'], capture_output=True, text=True, check=True)
        
        encoders_of_interest = list(ENCODER_INFO.keys())
        filtered_lines = [line for line in result.stdout.splitlines() if any(encoder in line for encoder in encoders_of_interest)]
        
        encoders_list = []
        for line in filtered_lines:
            for encoder in encoders_of_interest:
                if encoder in line:
                    user_friendly_name, container, presets = ENCODER_INFO.get(encoder, (encoder, 'unknown', []))
                    encoders_list.append((encoder, user_friendly_name, container, presets))
        
        # Filter out encoders that aren't supported by the current hardware
        available_encoders = []
        result_hwaccels = subprocess.run([ffmpeg_executable, '-hwaccels'], capture_output=True, text=True, check=True)
        hwaccels = result_hwaccels.stdout.lower()
        for encoder, name, container, presets in encoders_list:
            if ('qsv' in encoder and 'qsv' in hwaccels) or \
               ('nvenc' in encoder and 'cuda' in hwaccels) or \
               ('amf' in encoder and 'amf' in hwaccels) or \
               (encoder.startswith('lib') and 'qsv' not in encoder and 'nvenc' not in encoder and 'amf' not in encoder):
                available_encoders.append((encoder, name, container, presets))
        
        return available_encoders

    except FileNotFoundError as e:
        print(f"FileNotFoundError occurred: {e}")
        return []
    except subprocess.CalledProcessError as e:
        print(f"Error occurred while running FFmpeg: {e}")
        return []
