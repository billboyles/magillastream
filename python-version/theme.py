from PySide6 import QtWidgets

class ThemeManager:
    def __init__(self, root):
        self.root = root
        self.current_theme = {}

    def apply_theme(self, dark_mode):
        if dark_mode:
            theme_colors = {
                "entry_bg": "#333",
                "entry_fg": "#FFF",
                "button_bg": "#444",
                "button_fg": "#FFF",
                "checkbox_bg": "#1C1C1C",
                "checkbox_fg": "#FFF",
                "checkbox_border": "#555",
                "fg": "#FFF",
                "bg": "#1C1C1C",
                "active_bg": "#555",
                "active_fg": "#EEE"
            }
        else:
            theme_colors = {
                "entry_bg": "#FFF",
                "entry_fg": "#000",
                "button_bg": "#EEE",
                "button_fg": "#000",
                "checkbox_bg": "#D0D0D0",
                "checkbox_fg": "#000",
                "checkbox_border": "#888",
                "fg": "#000",
                "bg": "#D0D0D0",
                "active_bg": "#CCC",
                "active_fg": "#000"
            }

        self.current_theme = theme_colors
        self.update_widget_styles()

    def update_widget_styles(self):
        self.root.setStyleSheet(self.get_stylesheet())
        
        for child in self.root.findChildren(QtWidgets.QWidget):
            child.setStyleSheet(self.get_stylesheet())

    def get_stylesheet(self):
        return f"""
            QWidget {{
                background-color: {self.current_theme['bg']};
                color: {self.current_theme['fg']};
            }}
            QLineEdit {{
                background-color: {self.current_theme['entry_bg']};
                color: {self.current_theme['entry_fg']};
            }}
            QPushButton {{
                background-color: {self.current_theme['button_bg']};
                color: {self.current_theme['button_fg']};
            }}
            QPushButton:pressed {{
                background-color: {self.current_theme['active_bg']};
                color: {self.current_theme['active_fg']};
            }}
            QCheckBox {{
                color: {self.current_theme['checkbox_fg']};
            }}
            QCheckBox::indicator {{
                width: 15px;
                height: 15px;
            }}
            QCheckBox::indicator:unchecked {{
                background-color: {self.current_theme['checkbox_bg']};
                border: 2px solid {self.current_theme['checkbox_border']};
            }}
            QCheckBox::indicator:checked {{
                background-color: {self.current_theme['checkbox_bg']};
                border: 2px solid {self.current_theme['checkbox_border']};
                image: url(:/icons/checkmark.png);
            }}
            QPushButton#startButton, QPushButton#stopButton, QPushButton#saveButton {{
                padding: 10px 20px;
                font-size: 16px;
            }}
            QPushButton#youtubeKeyShowButton, QPushButton#twitchKeyShowButton {{
                max-width: 60px;
                max-height: 30px;
            }}
            QComboBox {{
                background-color: {self.current_theme['entry_bg']};
                color: {self.current_theme['entry_fg']};
            }}
        """