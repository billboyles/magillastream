import tkinter as tk
from tkinter import ttk, messagebox
from utils import list_ffmpeg_encoders
from theme import ThemeManager  # Import ThemeManager

class StreamManagerApp:
    def __init__(self, root, start_services_callback, stop_services_callback, save_config_callback, config):
        self.root = root
        self.root.title("MagillaStream Manager")
        self.root.geometry("900x500")  # Adjusted window size to accommodate new fields

        self.start_services_callback = start_services_callback
        self.stop_services_callback = stop_services_callback
        self.save_config_callback = save_config_callback
        self.ffmpeg_processes = None  # Initialize ffmpeg_processes
        self.config = config  # Store config for theme preference

        self.theme_manager = ThemeManager()  # Instantiate ThemeManager
        self.dark_mode = tk.BooleanVar(value=config.get("dark_mode", False))

        # Configure menu
        self.configure_menu()

        # Set initial theme
        self.theme_manager.apply_theme(self.root, self.dark_mode.get())

        # Title
        self.title_label = tk.Label(root, text="MagillaStream Manager", font=("Helvetica", 16))
        self.title_label.grid(row=0, columnspan=4, pady=10)

        # Configuration fields
        entry_width = 40  # Width of the entry fields

        # Nginx Fields
        self.create_label("Incoming App:", 1, 0)
        self.incoming_app = self.create_entry(1, 1, entry_width)

        self.create_label("Incoming Port:", 2, 0)
        self.incoming_port = self.create_entry(2, 1, entry_width)

        self.create_label("YouTube Ingest URL (Main):", 3, 0)
        self.youtube_ingest_main = self.create_entry(3, 1, entry_width)

        self.create_label("YouTube Ingest URL (Backup):", 4, 0)
        self.youtube_ingest_backup = self.create_entry(4, 1, entry_width)

        self.create_label("YouTube Stream Key:", 5, 0)
        self.youtube_stream_key = self.create_entry(5, 1, entry_width)

        self.create_label("Twitch Ingest URL:", 6, 0)
        self.twitch_ingest_url = self.create_entry(6, 1, entry_width)

        self.create_label("Twitch Stream Key:", 7, 0)
        self.twitch_stream_key = self.create_entry(7, 1, entry_width)

        # FFmpeg Video Fields
        self.create_label("Video Encoder:", 1, 2)
        self.ffmpeg_encoder = self.create_combobox(1, 3, entry_width)

        self.create_label("Resolution:", 2, 2)
        self.ffmpeg_resolution = self.create_combobox(2, 3, entry_width)
        self.populate_resolution_dropdown()  # Initialize resolution dropdown

        self.create_label("Preset:", 3, 2)
        self.ffmpeg_preset = self.create_combobox(3, 3, entry_width)

        self.create_label("Bitrate (kbps):", 4, 2)
        self.ffmpeg_bitrate = self.create_entry(4, 3, entry_width)

        self.create_label("Framerate (fps):", 5, 2)
        self.ffmpeg_framerate = self.create_entry(5, 3, entry_width)

        # FFmpeg Audio Fields
        self.create_label("Audio Encoder:", 6, 2)
        self.audio_encoder = self.create_combobox(6, 3, entry_width)
        self.populate_audio_encoder_dropdown()

        self.create_label("Audio Bitrate (kbps):", 7, 2)
        self.audio_bitrate = self.create_entry(7, 3, entry_width)
        self.audio_bitrate.insert(0, "192k")  # Default to 192k

        # Frame for buttons
        button_frame = tk.Frame(root, bg=self.theme_manager.current_theme["bg"])
        button_frame.grid(row=8, column=1, columnspan=3, pady=20, padx=10, sticky=tk.W)

        # Start, Stop, and Save buttons
        button_width = 20
        self.start_button = tk.Button(
            button_frame,
            text="Start Services",
            command=self.start_services,
            width=button_width,
            bg=self.theme_manager.current_theme["button_bg"],
            fg=self.theme_manager.current_theme["button_fg"],
            activebackground=self.theme_manager.current_theme["active_bg"],
            activeforeground=self.theme_manager.current_theme["active_fg"]
        )
        self.start_button.pack(side=tk.LEFT, padx=5)

        self.stop_button = tk.Button(
            button_frame,
            text="Stop Services",
            command=self.stop_services,
            width=button_width,
            bg=self.theme_manager.current_theme["button_bg"],
            fg=self.theme_manager.current_theme["button_fg"],
            activebackground=self.theme_manager.current_theme["active_bg"],
            activeforeground=self.theme_manager.current_theme["active_fg"]
        )
        self.stop_button.pack(side=tk.LEFT, padx=5)

        self.save_button = tk.Button(
            button_frame,
            text="Save Configuration",
            command=self.save_config,
            width=button_width,
            bg=self.theme_manager.current_theme["button_bg"],
            fg=self.theme_manager.current_theme["button_fg"],
            activebackground=self.theme_manager.current_theme["active_bg"],
            activeforeground=self.theme_manager.current_theme["active_fg"]
        )
        self.save_button.pack(side=tk.LEFT, padx=5)

        # Populate dropdowns before loading config
        self.populate_encoder_dropdown()

        # Load initial configuration
        self.load_config(config)

        # Bind the window close event
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)

    def configure_menu(self):
        menubar = tk.Menu(self.root)
        self.root.config(menu=menubar)

        options_menu = tk.Menu(menubar, tearoff=0)
        menubar.add_cascade(label="Options", menu=options_menu)
        options_menu.add_checkbutton(label="Dark Mode", variable=self.dark_mode, command=self.toggle_theme)

    def toggle_theme(self):
        self.theme_manager.apply_theme(self.root, self.dark_mode.get())

    def create_label(self, text, row, column):
        label = tk.Label(
            self.root,
            text=text,
            anchor=tk.E,
            bg=self.theme_manager.current_theme["bg"],
            fg=self.theme_manager.current_theme["fg"]
        )
        label.grid(row=row, column=column, sticky=tk.E, padx=10, pady=5)

    def create_entry(self, row, column, width):
        entry = tk.Entry(
            self.root,
            width=width,
            bg=self.theme_manager.current_theme["entry_bg"],
            fg=self.theme_manager.current_theme["entry_fg"],
            insertbackground=self.theme_manager.current_theme["fg"]
        )
        entry.grid(row=row, column=column, padx=10, pady=5)
        return entry

    def create_combobox(self, row, column, width):
        combobox = ttk.Combobox(
            self.root,
            width=width
        )
        combobox.grid(row=row, column=column, padx=10, pady=5)
        return combobox

    def populate_encoder_dropdown(self):
        encoders_list = list_ffmpeg_encoders()
        self.encoder_dict = {name: (code, container, presets) for code, name, container, presets in encoders_list}
        self.ffmpeg_encoder['values'] = list(self.encoder_dict.keys())

        if self.ffmpeg_encoder['values']:
            self.ffmpeg_encoder.set(self.ffmpeg_encoder['values'][0])
            self.populate_preset_dropdown()

        self.ffmpeg_encoder.bind("<<ComboboxSelected>>", self.on_encoder_change)

    def on_encoder_change(self, event):
        self.populate_preset_dropdown()

    def populate_preset_dropdown(self):
        encoder_name = self.ffmpeg_encoder.get()
        if encoder_name in self.encoder_dict:
            presets = self.encoder_dict[encoder_name][2]
            self.ffmpeg_preset['values'] = presets
            config_preset = self.ffmpeg_preset.get()
            if config_preset in presets:
                self.ffmpeg_preset.set(config_preset)
            else:
                self.ffmpeg_preset.set("medium")

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
        self.ffmpeg_resolution.set("720p")

    def populate_audio_encoder_dropdown(self):
        audio_encoders = {
            "AAC": "aac",
            "MP3": "libmp3lame",
            "Opus": "libopus"
        }
        self.audio_encoder_dict = audio_encoders
        self.audio_encoder['values'] = list(audio_encoders.keys())
        self.audio_encoder.set("AAC")

    def set_entry_value(self, entry_widget, value):
        entry_widget.delete(0, tk.END)
        entry_widget.insert(0, value)

    def load_config(self, config):
        self.incoming_app.insert(0, config.get("incoming_app", ""))
        self.incoming_port.insert(0, config.get("incoming_port", ""))
        self.youtube_ingest_main.insert(0, config.get("youtube_primary_ingest_url", ""))
        self.youtube_ingest_backup.insert(0, config.get("youtube_backup_ingest_url", ""))
        self.youtube_stream_key.insert(0, config.get("youtube_stream_key", ""))
        self.twitch_ingest_url.insert(0, config.get("twitch_ingest_url", ""))
        self.twitch_stream_key.insert(0, config.get("twitch_key", ""))

        encoder_data = config.get("ffmpeg_encoder", ('libx264', 'H.264 (libx264)', 'flv'))

        if isinstance(encoder_data, tuple) and len(encoder_data) >= 3:
            encoder_code, encoder_name, container = encoder_data[:3]
        else:
            encoder_code, encoder_name, container = ('libx264', 'H.264 (libx264)', 'flv')

        self.ffmpeg_encoder.set(encoder_name)
        self.encoder_dict = {name: (code, container, presets) for code, name, container, presets in list_ffmpeg_encoders()}

        if encoder_name not in self.encoder_dict:
            encoder_name = list(self.encoder_dict.keys())[0]
            self.ffmpeg_encoder.set(encoder_name)

        self.populate_preset_dropdown()

        preset_name = config.get("ffmpeg_preset", "medium")
        self.ffmpeg_preset.set(preset_name)

        self.ffmpeg_resolution.set(config.get("ffmpeg_resolution", "720p"))
        self.set_entry_value(self.ffmpeg_bitrate, config.get("ffmpeg_bitrate", "2500"))
        self.set_entry_value(self.ffmpeg_framerate, config.get("ffmpeg_framerate", "30"))

        audio_encoder_name = config.get("audio_encoder", "AAC")
        self.audio_encoder.set(audio_encoder_name)
        self.set_entry_value(self.audio_bitrate, config.get("audio_bitrate", "192k"))

    def validate_config(self, config):
        required_fields = ["incoming_app", "incoming_port", "youtube_primary_ingest_url", "youtube_stream_key", "twitch_ingest_url", "twitch_key"]
        for field in required_fields:
            if not config.get(field):
                messagebox.showwarning("Configuration Error", f"Please fill in the '{field.replace('_', ' ').capitalize()}' field.")
                return False
        return True

    def start_services(self):
        try:
            encoder_name = self.ffmpeg_encoder.get()
            encoder_code, container, _ = self.encoder_dict.get(encoder_name, ('libx264', 'flv', []))
            resolution_name = self.ffmpeg_resolution.get()
            resolution_value = self.resolution_dict.get(resolution_name, "1280x720")
            audio_encoder_name = self.audio_encoder.get()
            audio_encoder_code = self.audio_encoder_dict.get(audio_encoder_name, "aac")
            preset_name = self.ffmpeg_preset.get()
            config = {
                "incoming_app": self.incoming_app.get(),
                "incoming_port": self.incoming_port.get(),
                "youtube_primary_ingest_url": self.youtube_ingest_main.get(),
                "youtube_backup_ingest_url": self.youtube_ingest_backup.get(),
                "youtube_stream_key": self.youtube_stream_key.get(),
                "twitch_ingest_url": self.twitch_ingest_url.get(),
                "twitch_key": self.twitch_stream_key.get(),
                "ffmpeg_encoder": (encoder_code, encoder_name, container),
                "ffmpeg_resolution": resolution_value,
                "ffmpeg_bitrate": self.ffmpeg_bitrate.get(),
                "ffmpeg_framerate": self.ffmpeg_framerate.get(),
                "ffmpeg_preset": preset_name,
                "audio_encoder": audio_encoder_name,
                "audio_bitrate": self.audio_bitrate.get(),
                "dark_mode": self.dark_mode.get()
            }
            if not self.validate_config(config):
                return
            self.stop_services()
            self.ffmpeg_processes = self.start_services_callback(config)
        except Exception as e:
            messagebox.showerror("Error", f"Failed to start services: {e}")

    def stop_services(self):
        if self.ffmpeg_processes:
            self.stop_services_callback(self.ffmpeg_processes)
            self.ffmpeg_processes = None

    def save_config(self):
        encoder_name = self.ffmpeg_encoder.get()
        encoder_tuple = self.encoder_dict.get(encoder_name, ('libx264', 'H.264 (libx264)', 'flv'))

        if len(encoder_tuple) == 3:
            encoder_code, _, container = encoder_tuple
        else:
            encoder_code, container = encoder_tuple[0], encoder_tuple[1] if len(encoder_tuple) > 1 else 'flv'

        resolution_name = self.ffmpeg_resolution.get()
        resolution_value = self.resolution_dict.get(resolution_name, "1280x720")

        audio_encoder_name = self.audio_encoder.get()
        audio_encoder_code = self.audio_encoder_dict.get(audio_encoder_name, "aac")

        preset_name = self.ffmpeg_preset.get()

        config = {
            "incoming_app": self.incoming_app.get(),
            "incoming_port": self.incoming_port.get(),
            "youtube_primary_ingest_url": self.youtube_ingest_main.get(),
            "youtube_backup_ingest_url": self.youtube_ingest_backup.get(),
            "youtube_stream_key": self.youtube_stream_key.get(),
            "twitch_ingest_url": self.twitch_ingest_url.get(),
            "twitch_key": self.twitch_stream_key.get(),
            "ffmpeg_encoder": (encoder_code, encoder_name, container),
            "ffmpeg_resolution": resolution_value,
            "ffmpeg_bitrate": self.ffmpeg_bitrate.get(),
            "ffmpeg_framerate": self.ffmpeg_framerate.get(),
            "ffmpeg_preset": preset_name,
            "audio_encoder": audio_encoder_name,
            "audio_bitrate": self.audio_bitrate.get(),
            "dark_mode": self.dark_mode.get()
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
        self.ffmpeg_preset.config(state='disabled')
        self.audio_encoder.config(state='disabled')
        self.audio_bitrate.config(state='disabled')

    def on_closing(self):
        self.save_config()
        self.stop_services()
        self.root.destroy()
