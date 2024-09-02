# MagillaStream

## Overview

MagillaStream is a lightweight and portable RTMP server application that enables streamers to broadcast to YouTube and Twitch with customizable settings. It features a GUI for configuration and manages re-encoding as needed.

## Getting Started

You can use MagillaStream several ways - by using the prebuilt EXE file, by building your own EXE from the source code, or by running the source code using PYthon. 

## Using the EXE

1. Download the ZIP file [MagillaStream.zip](https://github.com/billboyles/magillastream/blob/main/MagillaStream.zip) containing the app EXE and other necessary components.

2. Unzip the file in a desired location, such as your home folder. 

3. Run the EXE (you may want to add a shortcut to your desktop).

## Using the Source Files

### Prerequisites

Ensure you have Python installed. You can download Python from [python.org](https://www.python.org/downloads/).

### Cloning the Repository

You can clone the repository using Git:

`git clone https://github.com/billboyles/magillastream.git`

Navigate to the project directory:

`cd magillastream`

Install the requirements:
`pip install -r requirements.txt`

### Building your own EXE

Using Pyinstaller to build the EXE with the following command:

`pyinstaller --onefile --add-data "ffmpeg;ffmpeg" --add-data "nginx;nginx" main.py    `

The resulting `dist` folder will contain the EXE. Copy the `nginx` and `ffmpeg` folders in the `dist` folder. This can then be renamed and moved as desired. 

### Running the Application Using Python

Run the main script:

`python main.py`

This will launch the application and open the GUI for configuration.

## Configuration

### Nginx Settings

In the GUI, you can configure the following Nginx settings:

- **Incoming App:** The application name to receive the incoming stream.
- **Incoming Port:** The port on which Nginx will listen for incoming streams.
- **YouTube Ingest URL (Main):** The main ingest URL for YouTube.
- **YouTube Ingest URL (Backup):** The backup ingest URL for YouTube.
- **YouTube Stream Key:** The stream key for YouTube.
- **Twitch Ingest URL:** The ingest URL for Twitch.
- **Twitch Stream Key:** The stream key for Twitch.

### FFmpeg Settings

In the GUI, you can configure the following FFmpeg settings:

- **Encoder:** Choose the FFmpeg encoder you want to use (e.g., `libx264`).
- **Resolution:** Set the resolution for the stream (e.g., `1920x1080`).
- **Bitrate (kbps):** Define the video bitrate (e.g., `6000`).
- **Framerate (fps):** Set the framerate for the stream (e.g., `30`).

## Nota Bene

**THIS IS A WORK IN PROGRESS!**

I do not guarantee there are no bugs, but I have tested this end to end (including deleting it and following my own setup instructions) and it seems to work. If you use this and encounter a bug (there will be bugs), PLEASE PLEASE PLEASE let me know. If you use it and it's great, optionally also tell me, that would be cool to hear.

## License

This project is licensed under the GNU General Public License (GPL) Version 3. See the [LICENSE](LICENSE) file for details.

