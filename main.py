import tkinter as tk
from config import load_config, save_config
from services import start_services, stop_services
from gui import StreamManagerApp 

def main():
    # Load the configuration
    config = load_config()

    # Create the main application window
    root = tk.Tk()
    app = StreamManagerApp(root, start_services, stop_services, save_config, config)

    try:
        # Start the Tkinter event loop
        root.mainloop()
    finally:
        # Ensure services are stopped when the application exits
        app.stop_services()

if __name__ == "__main__":
    main()
