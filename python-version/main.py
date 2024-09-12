import logging
import sys
from PySide6 import QtWidgets
from gui import StreamManagerApp
from config import load_config, save_config
from services import start_services, stop_services

# Logging setup
logging.basicConfig(
    level=logging.INFO,  # Set logging level to INFO
    format='%(asctime)s - %(levelname)s - %(message)s',  # Log format
    handlers=[logging.StreamHandler(sys.stdout)]  # Ensure logs are sent to stdout (console)
)

def main():
    logging.debug("Starting the application")
    app = QtWidgets.QApplication([])  # Initialize QApplication
    config = load_config()  # Load the configuration
    logging.debug(f"Loaded configuration: {config}")  

    # Create the main window and pass in the callbacks
    main_window = StreamManagerApp(
        start_services_callback=start_services,
        stop_services_callback=stop_services,
        save_config_callback=save_config,
        config=config
    )

    main_window.show()  # Show the GUI window
    logging.debug("Application is running")  # Log that the application is running
    app.exec()  # Start the application event loop

if __name__ == "__main__":
    main()
