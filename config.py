import os
import pickle
from pathlib import Path

# Function to get the configuration save path
def get_config_save_path():
    if os.name == 'nt':  # Windows
        return Path(os.getenv('APPDATA')) / "MagillaStream"
    else:  # For Linux/MacOS, use home directory
        return Path.home() / ".magillastream"

# Define the paths for saving configuration files
config_save_path = get_config_save_path()
config_save_path.mkdir(parents=True, exist_ok=True)
config_pickle_file = config_save_path / "config.pkl"

# Updated default configuration with encoder object, preset, and audio settings
default_config = {
    "youtube_stream_key": "YOUR_YOUTUBE_STREAM_KEY",
    "twitch_key": "YOUR_TWITCH_STREAM_KEY",
    "youtube_primary_ingest_url": "YOUTUBE_PRIMARY_INGEST_URL",
    "youtube_backup_ingest_url": "YOUTUBE_BACKUP_INGEST_URL",
    "twitch_ingest_url": "TWITCH_INGEST_URL",
    "incoming_port": "1935",
    "incoming_app": "live",
    "ffmpeg_encoder": ("libx264", "H.264 (libx264)", "flv"),  # Encoder object
    "ffmpeg_resolution": "1920x1080",
    "ffmpeg_bitrate": "6000",
    "ffmpeg_framerate": "30",
    "ffmpeg_preset": "medium",     # Default video preset
    "audio_encoder": "AAC",        # Default audio encoder
    "audio_bitrate": "192k"        # Default audio bitrate
}

def load_config():
    if config_pickle_file.exists():
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
                
                # Validate audio settings
                if "audio_encoder" not in loaded_config:
                    loaded_config["audio_encoder"] = default_config["audio_encoder"]
                if "audio_bitrate" not in loaded_config:
                    loaded_config["audio_bitrate"] = default_config["audio_bitrate"]

                # Validate preset
                if "ffmpeg_preset" not in loaded_config:
                    loaded_config["ffmpeg_preset"] = default_config["ffmpeg_preset"]
                
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
