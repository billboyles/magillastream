import os
import pickle

# Paths to configuration files
root_dir = os.path.dirname(__file__)
config_pickle_file = os.path.join(root_dir, 'config.pkl')

# Updated default configuration with encoder object
default_config = {
    "youtube_stream_key": "YOUR_YOUTUBE_STREAM_KEY",
    "twitch_key": "YOUR_TWITCH_STREAM_KEY",
    "youtube_primary_ingress_url": "YOUTUBE_PRIMARY_INGRESS_URL",
    "youtube_backup_ingress_url": "YOUTUBE_BACKUP_INGRESS_URL",
    "twitch_ingress_url": "TWITCH_INGRESS_URL",
    "incoming_port": "1935",
    "incoming_app": "live",
    "ffmpeg_encoder": ("libx264", "H.264 (libx264)", "flv"),  # Encoder object
    "ffmpeg_resolution": "1920x1080",
    "ffmpeg_bitrate": "6000",
    "ffmpeg_framerate": "30"
}

def load_config():
    if os.path.exists(config_pickle_file):
        try:
            with open(config_pickle_file, 'rb') as file:
                loaded_config = pickle.load(file)
                # Ensure all keys from default_config are present
                for key, value in default_config.items():
                    if key not in loaded_config:
                        loaded_config[key] = value
                
                # Validate encoder format
                if not isinstance(loaded_config.get("ffmpeg_encoder"), tuple) or len(loaded_config["ffmpeg_encoder"]) != 3:
                    loaded_config["ffmpeg_encoder"] = default_config["ffmpeg_encoder"]
                
                return loaded_config
        except (pickle.PickleError, IOError) as e:
            print(f"Error loading configuration: {e}")
            return default_config
    return default_config

def save_config(config):
    try:
        with open(config_pickle_file, 'wb') as file:
            pickle.dump(config, file)
    except (pickle.PickleError, IOError) as e:
        print(f"Error saving configuration: {e}")
