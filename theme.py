from PySide6 import QtCore, QtWidgets, QtGui

class ThemeManager:
    def __init__(self):
        self.current_theme = {}

    def apply_theme(self, root, dark_mode):
        if dark_mode:
            theme_colors = {
                "entry_bg": "#333",
                "entry_fg": "#FFF",
                "button_bg": "#444",
                "button_fg": "#FFF",
                "fg": "#FFF",
                "bg": "#222",
                "active_bg": "#555",
                "active_fg": "#EEE"
            }
        else:
            theme_colors = {
                "entry_bg": "#FFF",
                "entry_fg": "#000",
                "button_bg": "#DDD",
                "button_fg": "#000",
                "fg": "#000",
                "bg": "#EEE",
                "active_bg": "#CCC",
                "active_fg": "#000"
            }

        self.current_theme = theme_colors
        self.update_widget_styles(root)

    def update_widget_styles(self, root):
        # Apply theme to root window
        root.setStyleSheet(self.get_stylesheet())
        
        # Recursive styling if necessary
        for child in root.findChildren(QtWidgets.QWidget):
            child.setStyleSheet(self.get_stylesheet())

    def get_stylesheet(self):
        # Create stylesheet string
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
            QComboBox {{
                background-color: {self.current_theme['entry_bg']};
                color: {self.current_theme['entry_fg']};
            }}
        """

