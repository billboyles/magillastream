import tkinter as tk
from tkinter import ttk
from utils import list_ffmpeg_encoders  

class StreamManagerApp:
    def __init__(self, root, start_services_callback, stop_services_callback, save_config_callback, config):
        self.root = root
        self.root.title("Stream Manager")
        self.root.geometry("900x400")  # Overall window size

        self.start_services_callback = start_services_callback
        self.stop_services_callback = stop_services_callback
        self.save_config_callback = save_config_callback
        self.ffmpeg_processes = None  # Initialize ffmpeg_processes

        # Title
        tk.Label(root, text="MagillaStream Manager", font=("Helvetica", 16)).grid(row=0, columnspan=4, pady=10)

        # Configuration fields
        entry_width = 40  # Width of the entry fields

        # Nginx Fields
        tk.Label(root, text="Incoming App:", anchor=tk.E).grid(row=1, column=0, sticky=tk.E, padx=10, pady=5)
        self.incoming_app = tk.Entry(root, width=entry_width)
        self.incoming_app.grid(row=1, column=1, padx=10, pady=5)

        tk.Label(root, text="Incoming Port:", anchor=tk.E).grid(row=2, column=0, sticky=tk.E, padx=10, pady=5)
        self.incoming_port = tk.Entry(root, width=entry_width)
        self.incoming_port.grid(row=2, column=1, padx=10, pady=5)

        tk.Label(root, text="YouTube Ingest URL (Main):", anchor=tk.E).grid(row=3, column=0, sticky=tk.E, padx=10, pady=5)
        self.youtube_ingest_main = tk.Entry(root, width=entry_width)
        self.youtube_ingest_main.grid(row=3, column=1, padx=10, pady=5)

        tk.Label(root, text="YouTube Ingest URL (Backup):", anchor=tk.E).grid(row=4, column=0, sticky=tk.E, padx=10, pady=5)
        self.youtube_ingest_backup = tk.Entry(root, width=entry_width)
        self.youtube_ingest_backup.grid(row=4, column=1, padx=10, pady=5)

        tk.Label(root, text="YouTube Stream Key:", anchor=tk.E).grid(row=5, column=0, sticky=tk.E, padx=10, pady=5)
        self.youtube_stream_key = tk.Entry(root, width=entry_width)
        self.youtube_stream_key.grid(row=5, column=1, padx=10, pady=5)

        tk.Label(root, text="Twitch Ingest URL:", anchor=tk.E).grid(row=6, column=0, sticky=tk.E, padx=10, pady=5)
        self.twitch_ingest_url = tk.Entry(root, width=entry_width)
        self.twitch_ingest_url.grid(row=6, column=1, padx=10, pady=5)

        tk.Label(root, text="Twitch Stream Key:", anchor=tk.E).grid(row=7, column=0, sticky=tk.E, padx=10, pady=5)
        self.twitch_stream_key = tk.Entry(root, width=entry_width)
        self.twitch_stream_key.grid(row=7, column=1, padx=10, pady=5)

        # FFmpeg Fields
        tk.Label(root, text="Encoder:", anchor=tk.E).grid(row=1, column=2, sticky=tk.E, padx=10, pady=5)
        
        # Create a dropdown for encoders
        self.ffmpeg_encoder = ttk.Combobox(root, width=entry_width)
        self.ffmpeg_encoder.grid(row=1, column=3, padx=10, pady=5)
        self.populate_encoder_dropdown()

        tk.Label(root, text="Resolution:", anchor=tk.E).grid(row=2, column=2, sticky=tk.E, padx=10, pady=5)
        self.ffmpeg_resolution = ttk.Combobox(root, width=entry_width)
        self.ffmpeg_resolution.grid(row=2, column=3, padx=10, pady=5)
        self.populate_resolution_dropdown()

        tk.Label(root, text="Bitrate (kbps):", anchor=tk.E).grid(row=3, column=2, sticky=tk.E, padx=10, pady=5)
        self.ffmpeg_bitrate = tk.Entry(root, width=entry_width)
        self.ffmpeg_bitrate.grid(row=3, column=3, padx=10, pady=5)

        tk.Label(root, text="Framerate (fps):", anchor=tk.E).grid(row=4, column=2, sticky=tk.E, padx=10, pady=5)
        self.ffmpeg_framerate = tk.Entry(root, width=entry_width)
        self.ffmpeg_framerate.grid(row=4, column=3, padx=10, pady=5)

        # Frame for buttons
        button_frame = tk.Frame(root)
        button_frame.grid(row=8, column=1, columnspan=3, pady=10, padx=10, sticky=tk.W)

        # Start, Stop, and Save buttons
        button_width = 20
        tk.Button(button_frame, text="Start Services", command=self.start_services, width=button_width).pack(side=tk.LEFT, padx=5)
        tk.Button(button_frame, text="Stop Services", command=self.stop_services, width=button_width).pack(side=tk.LEFT, padx=5)
        tk.Button(button_frame, text="Save Configuration", command=self.save_config, width=button_width).pack(side=tk.LEFT, padx=5)

        # Load initial configuration
        self.load_config(config)

        # Bind the window close event
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)

    def populate_encoder_dropdown(self):
        encoders_list = list_ffmpeg_encoders()
        # Map human-readable names to tuples of (code, container)
        self.encoder_dict = {name: (code, container) for code, name, container in encoders_list}
        # Set dropdown values to user-friendly names
        self.ffmpeg_encoder['values'] = list(self.encoder_dict.keys())
        # Set default value if available
        current_encoder = self.ffmpeg_encoder.get()
        if current_encoder in self.encoder_dict:
            self.ffmpeg_encoder.set(current_encoder)
        else:
            self.ffmpeg_encoder.set(list(self.encoder_dict.keys())[0] if self.encoder_dict else '')

    def populate_resolution_dropdown(self):
        resolutions = {
            "360p": "640x360",
            "480p": "854x480",
            "720p": "1280x720",
            "1080p": "1920x1080",
            "1440p": "2560x1440",
            "4K": "3840x2160"
        }
        self.resolution_dict = resolutions
        self.ffmpeg_resolution['values'] = list(resolutions.keys())
        self.ffmpeg_resolution.set("720p")  # Set default resolution if available

    def mask_key(self, key):
        if len(key) > 4:
            return '*' * (len(key) - 4) + key[-4:]
        else:
            return key

    def load_config(self, config):
        self.incoming_app.insert(0, config.get("incoming_app", ""))
        self.incoming_port.insert(0, config.get("incoming_port", ""))
        self.youtube_ingest_main.insert(0, config.get("youtube_primary_ingest_url", ""))
        self.youtube_ingest_backup.insert(0, config.get("youtube_backup_ingest_url", ""))
        self.youtube_stream_key.insert(0, config.get("youtube_stream_key", ""))
        self.twitch_ingest_url.insert(0, config.get("twitch_ingest_url", ""))
        self.twitch_stream_key.insert(0, config.get("twitch_key", ""))

        # Fetch and debug encoder data
        encoder_data = config.get("ffmpeg_encoder", ('libx264', 'H.264 (libx264)', 'flv'))

        if isinstance(encoder_data, tuple) and len(encoder_data) == 3:
            encoder_code, encoder_name, container = encoder_data
        else:
            # Default values if the format is incorrect
            encoder_code, encoder_name, container = ('libx264', 'H.264 (libx264)', 'flv')

        self.ffmpeg_encoder.set(encoder_name)
        self.ffmpeg_resolution.set(config.get("ffmpeg_resolution", "720p"))
        self.ffmpeg_bitrate.insert(0, config.get("ffmpeg_bitrate", "2500"))
        self.ffmpeg_framerate.insert(0, config.get("ffmpeg_framerate", "30"))

    def start_services(self):
        encoder_name = self.ffmpeg_encoder.get()
        encoder_code, _ = self.encoder_dict.get(encoder_name, ('libx264', ''))
        resolution_name = self.ffmpeg_resolution.get()
        resolution_value = self.resolution_dict.get(resolution_name, "1280x720")  # Default to 720p
        config = {
            "incoming_app": self.incoming_app.get(),
            "incoming_port": self.incoming_port.get(),
            "youtube_primary_ingest_url": self.youtube_ingest_main.get(),
            "youtube_backup_ingest_url": self.youtube_ingest_backup.get(),
            "youtube_stream_key": self.youtube_stream_key.get(),
            "twitch_ingest_url": self.twitch_ingest_url.get(),
            "twitch_key": self.twitch_stream_key.get(),
            "ffmpeg_encoder": (encoder_code, encoder_name, self.encoder_dict.get(encoder_name, ('', ''))[1]),  # Full encoder object
            "ffmpeg_resolution": resolution_value,  # Pass resolution value for FFmpeg
            "ffmpeg_bitrate": self.ffmpeg_bitrate.get(),
            "ffmpeg_framerate": self.ffmpeg_framerate.get()
        }
        self.stop_services()  # Ensure no services are running before starting new ones
        self.ffmpeg_processes = self.start_services_callback(config)
   
    def stop_services(self):
        if self.ffmpeg_processes:
            self.stop_services_callback(self.ffmpeg_processes)
            self.ffmpeg_processes = None

    def save_config(self):
        encoder_name = self.ffmpeg_encoder.get()
        encoder_tuple = self.encoder_dict.get(encoder_name, ('libx264', 'H.264 (libx264)', 'flv'))
        
        # Ensure the encoder_tuple has exactly 3 elements
        if len(encoder_tuple) == 3:
            encoder_code, _, container = encoder_tuple
        else:
            # Handle unexpected format
            encoder_code, container = encoder_tuple[0], encoder_tuple[1] if len(encoder_tuple) > 1 else 'flv'
        
        resolution_name = self.ffmpeg_resolution.get()
        resolution_value = self.resolution_dict.get(resolution_name, "1280x720")  # Default to 720p
        
        config = {
            "incoming_app": self.incoming_app.get(),
            "incoming_port": self.incoming_port.get(),
            "youtube_primary_ingest_url": self.youtube_ingest_main.get(),
            "youtube_backup_ingest_url": self.youtube_ingest_backup.get(),
            "youtube_stream_key": self.youtube_stream_key.get(),
            "twitch_ingest_url": self.twitch_ingest_url.get(),
            "twitch_key": self.twitch_stream_key.get(),
            "ffmpeg_encoder": (encoder_code, encoder_name, container),
            "ffmpeg_resolution": resolution_value,  # Save resolution value
            "ffmpeg_bitrate": self.ffmpeg_bitrate.get(),
            "ffmpeg_framerate": self.ffmpeg_framerate.get()
        }
        self.save_config_callback(config)

    def disable_fields(self):
        self.incoming_app.config(state='disabled')
        self.incoming_port.config(state='disabled')
        self.youtube_ingest_main.config(state='disabled')
        self.youtube_ingest_backup.config(state='disabled')
        self.youtube_stream_key.config(state='disabled')
        self.twitch_ingest_url.config(state='disabled')
        self.twitch_stream_key.config(state='disabled')
        self.ffmpeg_encoder.config(state='disabled')
        self.ffmpeg_resolution.config(state='disabled')
        self.ffmpeg_bitrate.config(state='disabled')
        self.ffmpeg_framerate.config(state='disabled')

    def on_closing(self):
        self.stop_services()
        self.root.destroy()
