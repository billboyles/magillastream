import tkinter as tk
from tkinter import ttk

class ThemeManager:
    def __init__(self):
        self.light_theme = {
            "bg": "#f0f0f0",
            "fg": "#000000",
            "entry_bg": "#ffffff",
            "entry_fg": "#000000",
            "button_bg": "#e0e0e0",
            "button_fg": "#000000",
            "active_bg": "#d0d0d0",
            "active_fg": "#000000"
        }
        self.dark_theme = {
            "bg": "#333333",
            "fg": "#ffffff",
            "entry_bg": "#555555",
            "entry_fg": "#ffffff",
            "button_bg": "#444444",
            "button_fg": "#ffffff",
            "active_bg": "#555555",
            "active_fg": "#ffffff"
        }
        self.current_theme = self.light_theme

    def apply_theme(self, root, dark_mode):
        self.current_theme = self.dark_theme if dark_mode else self.light_theme
        root.configure(bg=self.current_theme["bg"])
        self.update_widget_styles(root)

    def update_widget_styles(self, root):
        for widget in root.winfo_children():
            self.style_widget(widget)
            if isinstance(widget, tk.Frame) or isinstance(widget, tk.LabelFrame):
                self.update_widget_styles(widget)

    def style_widget(self, widget):
        if isinstance(widget, tk.Entry):
            widget.configure(
                bg=self.current_theme["entry_bg"], 
                fg=self.current_theme["entry_fg"], 
                insertbackground=self.current_theme["fg"]
            )
        elif isinstance(widget, tk.Label):
            widget.configure(bg=self.current_theme["bg"], fg=self.current_theme["fg"])
        elif isinstance(widget, tk.Button):
            widget.configure(
                bg=self.current_theme["button_bg"], 
                fg=self.current_theme["button_fg"], 
                activebackground=self.current_theme["active_bg"], 
                activeforeground=self.current_theme["active_fg"]
            )
        elif isinstance(widget, ttk.Combobox):
            style = ttk.Style()
            style.theme_use('default')
            style.configure('TCombobox', 
                fieldbackground=self.current_theme["entry_bg"],
                background=self.current_theme["entry_bg"], 
                foreground=self.current_theme["entry_fg"]
            )

