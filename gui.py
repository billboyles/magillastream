from PySide6 import QtWidgets, QtGui, QtCore
import logging  # Import logging module
from utils import list_ffmpeg_encoders
from theme import ThemeManager

class StreamManagerApp(QtWidgets.QWidget):
    def __init__(self, parent=None, start_services_callback=None, stop_services_callback=None, save_config_callback=None, config=None):
        super().__init__(parent)
        self.start_services_callback = start_services_callback
        self.stop_services_callback = stop_services_callback
        self.save_config_callback = save_config_callback
        self.config = config
        self.ffmpeg_processes = None

        self.theme_manager = ThemeManager(self)
        self.dark_mode = config.get("dark_mode", False)

        self.init_ui()
        self.apply_theme()
        self.populate_encoder_dropdown()
        self.populate_resolution_dropdown()
        self.populate_audio_encoder_dropdown()
        self.load_config(self.config)

    def init_ui(self):
        self.setWindowTitle("MagillaStream Manager")
        self.resize(900, 500)
        layout = QtWidgets.QGridLayout(self)

        # Add widgets for configuration fields
        self.create_widgets(layout)
        self.setLayout(layout)

    def create_widgets(self, layout):
        # Set column width for input fields
        input_field_width = 350

        # Nginx-related fields
        nginx_labels = {
            "Incoming App:": QtWidgets.QLineEdit(),
            "Incoming Port:": QtWidgets.QLineEdit(),
            "YouTube Ingest URL (Main):": QtWidgets.QLineEdit(),
            "YouTube Ingest URL (Backup):": QtWidgets.QLineEdit(),
            "YouTube Stream Key:": QtWidgets.QLineEdit(),
            "Twitch Ingest URL:": QtWidgets.QLineEdit(),
            "Twitch Stream Key:": QtWidgets.QLineEdit(),
        }

        # FFmpeg-related fields
        ffmpeg_labels = {
            "Video Encoder:": QtWidgets.QComboBox(),
            "Resolution:": QtWidgets.QComboBox(),
            "Preset:": QtWidgets.QComboBox(),
            "Bitrate (kbps):": QtWidgets.QLineEdit(),
            "Framerate (fps):": QtWidgets.QLineEdit(),
            "Audio Encoder:": QtWidgets.QComboBox(),
            "Audio Bitrate (kbps):": QtWidgets.QLineEdit()
        }

        # Add Nginx fields (left column)
        row = 0
        for label_text, widget in nginx_labels.items():
            label = QtWidgets.QLabel(label_text)
            layout.addWidget(label, row, 0)
            widget.setFixedWidth(input_field_width)  # Set width 25% wider
            layout.addWidget(widget, row, 1)
            row += 1

        # Show buttons
        self.youtube_key_show_button = QtWidgets.QPushButton("Show")
        self.twitch_key_show_button = QtWidgets.QPushButton("Show")

        self.youtube_key_show_button.setObjectName("youtubeKeyShowButton")
        self.twitch_key_show_button.setObjectName("twitchKeyShowButton")

        self.youtube_key_show_button.setCheckable(True)
        self.twitch_key_show_button.setCheckable(True)

        layout.addWidget(self.youtube_key_show_button, 4, 2)
        layout.addWidget(self.twitch_key_show_button, 6, 2)

        # Connect the "Show" buttons to the masking toggle function
        self.youtube_key_show_button.toggled.connect(lambda checked: self.toggle_masking(self.youtube_stream_key, checked))
        self.twitch_key_show_button.toggled.connect(lambda checked: self.toggle_masking(self.twitch_stream_key, checked))

        # Add FFmpeg fields (right column)
        row = 0
        for label_text, widget in ffmpeg_labels.items():
            label = QtWidgets.QLabel(label_text)
            layout.addWidget(label, row, 3)
            if isinstance(widget, QtWidgets.QLineEdit):
                widget.setFixedWidth(input_field_width)  # Set width 25% wider
            layout.addWidget(widget, row, 4)
            row += 1

        # Main buttons
        self.start_button = QtWidgets.QPushButton("Start Services")
        self.stop_button = QtWidgets.QPushButton("Stop Services")
        self.save_button = QtWidgets.QPushButton("Save Configuration")

        self.stop_button.setObjectName("stopButton")
        self.start_button.setObjectName("startButton")
        self.save_button.setObjectName("saveButton")

        self.start_button.clicked.connect(self.start_services)
        self.stop_button.clicked.connect(self.stop_services)
        self.save_button.clicked.connect(self.save_config)

        # Adjust column spans to reduce negative space and reverse Stop and Save buttons
        layout.addWidget(self.start_button, row, 0, 1, 2)  # Span across columns 0 and 1
        layout.addWidget(self.save_button, row, 2)  # Moved to the position of the Stop button
        layout.addWidget(self.stop_button, row, 3, 1, 2)  # Span across columns 3 and 4

        # Theme toggle
        self.theme_toggle = QtWidgets.QCheckBox("Dark Mode")
        self.theme_toggle.setChecked(self.dark_mode)
        layout.addWidget(self.theme_toggle, row, 5)
        self.theme_toggle.toggled.connect(self.toggle_theme)

        # Store references for later use
        self.incoming_app = nginx_labels["Incoming App:"]
        self.incoming_port = nginx_labels["Incoming Port:"]
        self.youtube_ingest_main = nginx_labels["YouTube Ingest URL (Main):"]
        self.youtube_ingest_backup = nginx_labels["YouTube Ingest URL (Backup):"]
        self.youtube_stream_key = nginx_labels["YouTube Stream Key:"]
        self.twitch_ingest_url = nginx_labels["Twitch Ingest URL:"]
        self.twitch_stream_key = nginx_labels["Twitch Stream Key:"]
        self.ffmpeg_encoder = ffmpeg_labels["Video Encoder:"]
        self.ffmpeg_resolution = ffmpeg_labels["Resolution:"]
        self.ffmpeg_preset = ffmpeg_labels["Preset:"]
        self.ffmpeg_bitrate = ffmpeg_labels["Bitrate (kbps):"]
        self.ffmpeg_framerate = ffmpeg_labels["Framerate (fps):"]
        self.audio_encoder = ffmpeg_labels["Audio Encoder:"]
        self.audio_bitrate = ffmpeg_labels["Audio Bitrate (kbps):"]

        # Set initial masking for keys
        self.youtube_stream_key.setEchoMode(QtWidgets.QLineEdit.Password)
        self.twitch_stream_key.setEchoMode(QtWidgets.QLineEdit.Password)

    def apply_theme(self):
        self.theme_manager.apply_theme(self.dark_mode)

    def toggle_theme(self, checked):
        self.dark_mode = checked
        self.apply_theme()

    def populate_encoder_dropdown(self):
        encoders_list = list_ffmpeg_encoders()
        self.encoder_dict = {name: (code, container, presets) for code, name, container, presets in encoders_list}
        for name in self.encoder_dict.keys():
            self.ffmpeg_encoder.addItem(name)

        self.ffmpeg_encoder.currentIndexChanged.connect(self.on_encoder_change)
        self.populate_preset_dropdown()

    def on_encoder_change(self):
        self.populate_preset_dropdown()

    def populate_preset_dropdown(self):
        encoder_name = self.ffmpeg_encoder.currentText()
        presets = self.encoder_dict[encoder_name][2]
        self.ffmpeg_preset.clear()
        self.ffmpeg_preset.addItems(presets)

    def populate_resolution_dropdown(self):
        resolutions = ["360p", "480p", "720p", "1080p", "1440p", "4K"]
        self.ffmpeg_resolution.addItems(resolutions)

    def populate_audio_encoder_dropdown(self):
        audio_encoders = ["AAC", "MP3", "Opus"]
        self.audio_encoder.addItems(audio_encoders)

    def toggle_masking(self, line_edit, checked):
        if checked:
            line_edit.setEchoMode(QtWidgets.QLineEdit.Normal)
        else:
            line_edit.setEchoMode(QtWidgets.QLineEdit.Password)

    def load_config(self, config):
        # Load saved config or default values
        self.incoming_app.setText(config.get("incoming_app", ""))
        self.incoming_port.setText(config.get("incoming_port", ""))
        self.youtube_ingest_main.setText(config.get("youtube_primary_ingest_url", ""))
        self.youtube_ingest_backup.setText(config.get("youtube_backup_ingest_url", ""))
        self.youtube_stream_key.setText(config.get("youtube_stream_key", ""))
        self.twitch_ingest_url.setText(config.get("twitch_ingest_url", ""))
        self.twitch_stream_key.setText(config.get("twitch_key", ""))

        # Only pass the encoder name (not a tuple) to setCurrentText
        encoder_data = config.get("ffmpeg_encoder", ("", ""))
        encoder_name = encoder_data[1]  # Extract the encoder name
        self.ffmpeg_encoder.setCurrentText(encoder_name)

        # Set other config values
        self.populate_preset_dropdown()
        self.ffmpeg_resolution.setCurrentText(config.get("ffmpeg_resolution", "1080p"))
        self.ffmpeg_bitrate.setText(config.get("ffmpeg_bitrate", "6000"))
        self.ffmpeg_framerate.setText(config.get("ffmpeg_framerate", "30"))
        self.audio_encoder.setCurrentText(config.get("audio_encoder", "AAC"))
        self.audio_bitrate.setText(config.get("audio_bitrate", "192"))

    def save_config(self):
        self.config = {
            "incoming_app": self.incoming_app.text(),
            "incoming_port": self.incoming_port.text(),
            "youtube_primary_ingest_url": self.youtube_ingest_main.text(),
            "youtube_backup_ingest_url": self.youtube_ingest_backup.text(),
            "youtube_stream_key": self.youtube_stream_key.text(),
            "twitch_ingest_url": self.twitch_ingest_url.text(),
            "twitch_key": self.twitch_stream_key.text(),
            "ffmpeg_encoder": (
                self.encoder_dict[self.ffmpeg_encoder.currentText()][0],  # encoder code
                self.ffmpeg_encoder.currentText(),  # encoder name
                self.ffmpeg_resolution.currentText()  # selected resolution
            ),
            "ffmpeg_resolution": self.ffmpeg_resolution.currentText(),
            "ffmpeg_bitrate": self.ffmpeg_bitrate.text(),
            "ffmpeg_framerate": self.ffmpeg_framerate.text(),
            "audio_encoder": self.audio_encoder.currentText(),
            "audio_bitrate": self.audio_bitrate.text(),
            "dark_mode": self.dark_mode
        }

        # Call the save configuration callback to persist the data
        if self.save_config_callback:
            self.save_config_callback(self.config)

    def start_services(self):
        logging.debug("Starting services...")
        self.save_config()  # Save current config before starting services
        self.ffmpeg_processes = self.start_services_callback(self.config)  # Store FFmpeg processes
        if self.ffmpeg_processes:
            logging.debug("Services started, disabling fields")
            self.disable_fields(True)
        else:
            logging.error("Failed to start services")

    def stop_services(self):
        logging.debug("Stopping services...")
        if self.ffmpeg_processes:
            self.stop_services_callback(self.ffmpeg_processes)  # Pass FFmpeg processes to stop
            self.ffmpeg_processes = None  # Reset after stopping services
            logging.debug("Services stopped, enabling fields")
        self.disable_fields(False)

    def disable_fields(self, disable):
        fields = [self.incoming_app, self.incoming_port, self.youtube_ingest_main, self.youtube_ingest_backup,
                  self.youtube_stream_key, self.twitch_ingest_url, self.twitch_stream_key, self.ffmpeg_encoder,
                  self.ffmpeg_resolution, self.ffmpeg_preset, self.ffmpeg_bitrate, self.ffmpeg_framerate,
                  self.audio_encoder, self.audio_bitrate, self.save_button, self.start_button]
        for field in fields:
            field.setDisabled(disable)

    def closeEvent(self, event):
        logging.debug("Closing application")
        if self.ffmpeg_processes:
            self.stop_services_callback(self.ffmpeg_processes)  # Stop services on close
        self.save_config()  # Save the config before closing
        event.accept()
