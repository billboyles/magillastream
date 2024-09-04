import tkinter as tk
from tkinter import ttk

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
                "active_fg": "#EEE"  # Added active_fg key
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
                "active_fg": "#000"  # Added active_fg key
            }

        self.current_theme = theme_colors
        self.update_widget_styles(root)

    def update_widget_styles(self, root):
        for widget in root.winfo_children():
            self.style_widget(widget)
            if isinstance(widget, ttk.Widget):
                self.style_ttk_widget(widget)
            if widget.winfo_children():
                self.update_widget_styles(widget)

    def style_widget(self, widget):
        widget_type = str(type(widget))
        print(f"Styling widget of type: {widget_type}")
        print(f"Theme values: {self.current_theme}")
        try:
            if isinstance(widget, (tk.Entry, tk.Text)):
                widget.configure(
                    background=self.current_theme.get("entry_bg", "#FFF"),
                    foreground=self.current_theme.get("entry_fg", "#000")
                )
            elif isinstance(widget, (tk.Label, tk.Button)):
                widget.configure(
                    background=self.current_theme.get("bg", "#EEE"),
                    foreground=self.current_theme.get("fg", "#000")
                )
            else:
                widget.configure(
                    bg=self.current_theme.get("bg", "#EEE"),
                    fg=self.current_theme.get("fg", "#000")
                )
        except Exception as e:
            print(f"Error styling widget: {e}")

    def style_ttk_widget(self, widget):
        style = ttk.Style()
        widget_class = widget.winfo_class()
        if widget_class == "TButton":
            style.configure("TButton",
                background=self.current_theme.get("button_bg", "#DDD"),
                foreground=self.current_theme.get("button_fg", "#000"),
                relief="flat")  # Ensure the background color is applied
        elif widget_class == "TCombobox":
            style.configure("TCombobox",
                background=self.current_theme.get("entry_bg", "#FFF"),
                foreground=self.current_theme.get("entry_fg", "#000"))
        # Add more styles as needed
