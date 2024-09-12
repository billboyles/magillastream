import os
import sys

def resource_path(relative_path):
    try:
        # When running as a packaged executable, use the directory of the exe
        base_path = sys._MEIPASS
    except AttributeError:
        # If not running as an executable, use the absolute path of the current directory
        base_path = os.path.abspath(os.path.dirname(__file__))

    return os.path.join(base_path, relative_path)

# Define paths using resource_path
nginx_path = resource_path('nginx')
ffmpeg_path = resource_path('ffmpeg')
conf_dir = os.path.join(nginx_path, 'conf')
nginx_conf_file = os.path.join(conf_dir, 'nginx.conf')
rtmp_conf_file = os.path.join(conf_dir, 'rtmp.conf')
dynamic_rtmp_template_file = os.path.join(conf_dir, 'rtmp.conf.template')
dynamic_rtmp_file = os.path.join(conf_dir, 'dynamic_rtmp.conf')
logs_dir = os.path.join(nginx_path, 'logs')

# Ensure necessary directories exist
os.makedirs(conf_dir, exist_ok=True)
os.makedirs(logs_dir, exist_ok=True)

temp_dirs = [
    os.path.join(nginx_path, 'temp'),
    os.path.join(nginx_path, 'temp', 'client_body_temp'),
    os.path.join(nginx_path, 'temp', 'proxy_temp'),
    os.path.join(nginx_path, 'temp', 'fastcgi_temp'),
    os.path.join(nginx_path, 'temp', 'uwsgi_temp'),
    os.path.join(nginx_path, 'temp', 'scgi_temp')
]
for temp_dir in temp_dirs:
    os.makedirs(temp_dir, exist_ok=True)
