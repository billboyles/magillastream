import tkinter as tk
from tkinter import ttk, messagebox
from utils import list_ffmpeg_encoders
from config import save_config  # Assuming save_config is available here

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

        # Theme variables
        self.style = ttk.Style()
        self.dark_mode = tk.BooleanVar(value=config.get("dark_mode", False))

        # Configure menu
        self.configure_menu()

        # Define color schemes
        self.light_theme = {
            "bg": "#FFFFFF",
            "fg": "#000000",
            "entry_bg": "#FFFFFF",
            "entry_fg": "#000000",
            "highlight_bg": "#E0E0E0",
            "button_bg": "#F0F0F0",
            "button_fg": "#000000",
            "combobox_bg": "#FFFFFF",
            "combobox_fg": "#000000",
            "active_bg": "#D9D9D9",
            "active_fg": "#000000"
        }

        self.dark_theme = {
            "bg": "#2D2D2D",
            "fg": "#FFFFFF",
            "entry_bg": "#3C3F41",
            "entry_fg": "#FFFFFF",
            "highlight_bg": "#3C3F41",
            "button_bg": "#3C3F41",
            "button_fg": "#FFFFFF",
            "combobox_bg": "#3C3F41",
            "combobox_fg": "#FFFFFF",
            "active_bg": "#5C5C5C",
            "active_fg": "#FFFFFF"
        }

        # Set initial theme
        self.apply_theme()

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
        button_frame = tk.Frame(root, bg=self.current_theme["bg"])
        button_frame.grid(row=8, column=1, columnspan=3, pady=20, padx=10, sticky=tk.W)

        # Start, Stop, and Save buttons
        button_width = 20
        self.start_button = tk.Button(
            button_frame,
            text="Start Services",
            command=self.start_services,
            width=button_width,
            bg=self.current_theme["button_bg"],
            fg=self.current_theme["button_fg"],
            activebackground=self.current_theme["active_bg"],
            activeforeground=self.current_theme["active_fg"]
        )
        self.start_button.pack(side=tk.LEFT, padx=5)

        self.stop_button = tk.Button(
            button_frame,
            text="Stop Services",
            command=self.stop_services,
            width=button_width,
            bg=self.current_theme["button_bg"],
            fg=self.current_theme["button_fg"],
            activebackground=self.current_theme["active_bg"],
            activeforeground=self.current_theme["active_fg"]
        )
        self.stop_button.pack(side=tk.LEFT, padx=5)

        self.save_button = tk.Button(
            button_frame,
            text="Save Configuration",
            command=self.save_config,
            width=button_width,
            bg=self.current_theme["button_bg"],
            fg=self.current_theme["button_fg"],
            activebackground=self.current_theme["active_bg"],
            activeforeground=self.current_theme["active_fg"]
        )
        self.save_button.pack(side=tk.LEFT, padx=5)

        # Populate dropdowns before loading config
        self.populate_encoder_dropdown()

        # Load initial configuration
        self.load_config(config)

        # Bind the window close event
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)

    def configure_menu(self):
        """Configure the top menu bar with theme toggle."""
        menubar = tk.Menu(self.root)
        self.root.config(menu=menubar)

        # View menu
        view_menu = tk.Menu(menubar, tearoff=0)
        menubar.add_cascade(label="View", menu=view_menu)
        view_menu.add_checkbutton(
            label="Dark Mode",
            variable=self.dark_mode,
            command=self.toggle_theme
        )

    def toggle_theme(self):
        """Toggle between dark and light themes."""
        self.apply_theme()
        # Save theme preference
        self.config["dark_mode"] = self.dark_mode.get()
        self.save_config_callback(self.config)

    def apply_theme(self):
        """Apply the selected theme to all widgets."""
        self.current_theme = self.dark_theme if self.dark_mode.get() else self.light_theme
        self.root.configure(bg=self.current_theme["bg"])
        self.style.theme_use('default')
        self.style.configure('TLabel', background=self.current_theme["bg"], foreground=self.current_theme["fg"])
        self.style.configure('TEntry', fieldbackground=self.current_theme["entry_bg"], foreground=self.current_theme["entry_fg"])
        self.style.configure('TCombobox', fieldbackground=self.current_theme["combobox_bg"], foreground=self.current_theme["combobox_fg"])
        self.style.map('TCombobox', fieldbackground=[('readonly', self.current_theme["combobox_bg"])], foreground=[('readonly', self.current_theme["combobox_fg"])])
        self.update_widget_styles()

    def update_widget_styles(self):
        """Update styles for all widgets based on the current theme."""
        widgets = self.root.winfo_children()
        for widget in widgets:
            self.style_widget(widget)

    def style_widget(self, widget):
        """Apply styles to individual widgets."""
        if isinstance(widget, tk.Frame):
            widget.configure(bg=self.current_theme["bg"])
            for child in widget.winfo_children():
                self.style_widget(child)
        elif isinstance(widget, tk.Label):
            widget.configure(bg=self.current_theme["bg"], fg=self.current_theme["fg"])
        elif isinstance(widget, tk.Entry):
            widget.configure(bg=self.current_theme["entry_bg"], fg=self.current_theme["entry_fg"], insertbackground=self.current_theme["fg"])
        elif isinstance(widget, ttk.Combobox):
            widget.configure(style='TCombobox')
            widget.configure(background=self.current_theme["combobox_bg"], foreground=self.current_theme["combobox_fg"])
        elif isinstance(widget, tk.Button):
            widget.configure(
                bg=self.current_theme["button_bg"],
                fg=self.current_theme["button_fg"],
                activebackground=self.current_theme["active_bg"],
                activeforeground=self.current_theme["active_fg"]
            )

    def create_label(self, text, row, column):
        label = tk.Label(
            self.root,
            text=text,
            anchor=tk.E,
            bg=self.current_theme["bg"],
            fg=self.current_theme["fg"]
        )
        label.grid(row=row, column=column, sticky=tk.E, padx=10, pady=5)

    def create_entry(self, row, column, width):
        entry = tk.Entry(
            self.root,
            width=width,
            bg=self.current_theme["entry_bg"],
            fg=self.current_theme["entry_fg"],
            insertbackground=self.current_theme["fg"]
        )
        entry.grid(row=row, column=column, padx=10, pady=5)
        return entry

    def create_combobox(self, row, column, width):
        combobox = ttk.Combobox(
            self.root,
            width=width,
            state='readonly',
            style='TCombobox'
        )
        combobox.grid(row=row, column=column, padx=10, pady=5)
        return combobox

    def populate_encoder_dropdown(self):
        encoders_list = list_ffmpeg_encoders()
        # Map human-readable names to tuples of (code, container, presets)
        self.encoder_dict = {name: (code, container, presets) for code, name, container, presets in encoders_list}
        # Set dropdown values to user-friendly names
        encoder_names = list(self.encoder_dict.keys())
        self.ffmpeg_encoder['values'] = encoder_names
        # Set default value if available
        if encoder_names:
            selected_encoder = self.ffmpeg_encoder.get()
            if selected_encoder not in encoder_names:
                selected_encoder = encoder_names[0]
            self.ffmpeg_encoder.set(selected_encoder)
            self.populate_preset_dropdown()  # Populate preset dropdown based on the selected encoder

        # Bind the change event to update presets when the encoder changes
        self.ffmpeg_encoder.bind("<<ComboboxSelected>>", self.on_encoder_change)

    def on_encoder_change(self, event):
        self.populate_preset_dropdown()

    def populate_preset_dropdown(self):
        encoder_name = self.ffmpeg_encoder.get()
        if encoder_name in self.encoder_dict:
            presets = self.encoder_dict[encoder_name][2]  # Retrieve presets from the encoder_dict
            self.ffmpeg_preset['values'] = presets
            # Set default preset
            self.ffmpeg_preset.set(presets[0] if presets else '')

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
        resolution_options = list(resolutions.keys())
        self.ffmpeg_resolution['values'] = resolution_options
        self.ffmpeg_resolution.set("720p")  # Set default resolution if available

    def populate_audio_encoder_dropdown(self):
        audio_encoders = {
            "AAC": "aac",
            "MP3": "libmp3lame",
            "Opus": "libopus"
        }
        self.audio_encoder_dict = audio_encoders
        audio_options = list(audio_encoders.keys())
        self.audio_encoder['values'] = audio_options
        self.audio_encoder.set("AAC")  # Default to AAC

    def set_entry_value(self, entry_widget, value):
        entry_widget.delete(0, tk.END)
        entry_widget.insert(0, value)

    def load_config(self, config):
        self.set_entry_value(self.incoming_app, config.get("incoming_app", ""))
        self.set_entry_value(self.incoming_port, config.get("incoming_port", ""))
        self.set_entry_value(self.youtube_ingest_main, config.get("youtube_primary_ingest_url", ""))
        self.set_entry_value(self.youtube_ingest_backup, config.get("youtube_backup_ingest_url", ""))
        self.set_entry_value(self.youtube_stream_key, config.get("youtube_stream_key", ""))
        self.set_entry_value(self.twitch_ingest_url, config.get("twitch_ingest_url", ""))
        self.set_entry_value(self.twitch_stream_key, config.get("twitch_key", ""))

        # Fetch and validate encoder data
        encoder_data = config.get("ffmpeg_encoder")
        if encoder_data:
            encoder_code, encoder_name, container = encoder_data
            if encoder_name in self.encoder_dict:
                self.ffmpeg_encoder.set(encoder_name)
                self.populate_preset_dropdown()
            else:
                self.ffmpeg_encoder.set(list(self.encoder_dict.keys())[0])
                self.populate_preset_dropdown()
        else:
            self.ffmpeg_encoder.set(list(self.encoder_dict.keys())[0])
            self.populate_preset_dropdown()

        # Set the preset value
        preset_name = config.get("ffmpeg_preset")
        if preset_name and preset_name in self.ffmpeg_preset['values']:
            self.ffmpeg_preset.set(preset_name)
        else:
            self.ffmpeg_preset.set(self.ffmpeg_preset['values'][0])

        # Set other fields
        resolution_value = config.get("ffmpeg_resolution", "720p")
        if resolution_value in self.ffmpeg_resolution['values']:
            self.ffmpeg_resolution.set(resolution_value)
        else:
            self.ffmpeg_resolution.set("720p")

        self.set_entry_value(self.ffmpeg_bitrate, config.get("ffmpeg_bitrate", "2500"))
        self.set_entry_value(self.ffmpeg_framerate, config.get("ffmpeg_framerate", "30"))

        # Load audio settings
        audio_encoder_name = config.get("audio_encoder", "AAC")
        if audio_encoder_name in self.audio_encoder['values']:
            self.audio_encoder.set(audio_encoder_name)
        else:
            self.audio_encoder.set("AAC")

        self.set_entry_value(self.audio_bitrate, config.get("audio_bitrate", "192k"))

    def validate_config(self, config):
        required_fields = [
            "incoming_app",
            "incoming_port",
            "youtube_primary_ingest_url",
            "youtube_stream_key",
            "twitch_ingest_url",
            "twitch_key"
        ]
        for field in required_fields:
            if not config.get(field):
                messagebox.showwarning(
                    "Configuration Error",
                    f"Please fill in the '{field.replace('_', ' ').capitalize()}' field."
                )
                return False
        return True

    def start_services(self):
        try:
            encoder_name = self.ffmpeg_encoder.get()
            encoder_code, container, _ = self.encoder_dict.get(encoder_name, ('libx264', 'flv', []))
            resolution_name = self.ffmpeg_resolution.get()
            resolution_value = self.resolution_dict.get(resolution_name, "1280x720")  # Default to 720p
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
            self.stop_services()  # Ensure no services are running before starting new ones
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

        resolution_name = self.ffmpeg_resolution.get()
        resolution_value = self.resolution_dict.get(resolution_name, "1280x720")  # Default to 720p

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
            "ffmpeg_encoder": encoder_tuple,
            "ffmpeg_resolution": resolution_value,  # Save resolution value
            "ffmpeg_bitrate": self.ffmpeg_bitrate.get(),
            "ffmpeg_framerate": self.ffmpeg_framerate.get(),
            "ffmpeg_preset": preset_name,
            "audio_encoder": audio_encoder_name,
            "audio_bitrate": self.audio_bitrate.get(),
            "dark_mode": self.dark_mode.get()
        }
        self.save_config_callback(config)
        messagebox.showinfo("Configuration Saved", "Your configuration has been saved successfully.")

    def on_closing(self):
        self.save_config()  # Auto-save configuration on close
        self.stop_services()
        self.root.destroy()
