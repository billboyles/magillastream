# MagillaStream

## Overview

MagillaStream is a lightweight and portable RTMP server application that enables streamers to broadcast to YouTube and Twitch with customizable settings. It features a GUI for configuration and manages re-encoding as needed.

## Getting Started

### Prerequisites

Ensure you have Python installed. You can download Python from [python.org](https://www.python.org/downloads/).

### Cloning the Repository

You can clone the repository using Git:

`git clone https://github.com/billboyles/magillastream.git`

Alternatively, you can download the ZIP file of the repository from GitHub and extract it to your desired location.
# MagillaStream

## Overview

MagillaStream is a lightweight and portable RTMP server application that enables streamers to broadcast to YouTube and Twitch with customizable settings. It features a GUI for configuration and manages re-encoding as needed.

## Getting Started

### Prerequisites

Ensure you have Python installed. You can download Python from [python.org](https://www.python.org/downloads/).

### Cloning the Repository

You can clone the repository using Git:

`git clone https://github.com/billboyles/magillastream.git`

Alternatively, you can download the ZIP file of the repository from GitHub and extract it to your desired location.

### Running the Application

Navigate to the project directory:

`cd magillastream`

Install the requirements:
`pip install -r requirements.txt`

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
THIS IS A WORK IN PROGRESS!

I do not guarantee there are no bugs, but I have tested this end to end (including deleting it and following my own setup instructions) and it seems to work. If you use this and encounter a bug (there will be bugs), PLEASE PLEASE PLEASE let me know. If you use it and it's great, optionally also tell me, that would be cool to hear.

I am currently having a little trouble with the re-encoded stream not quite keeping up (it's like 99.5%) but I don't think the issue is my GPU because it is barely even working. I think that may have to do with my actual OBS settings. I'm not a streamer and I'm probably messing something up. If you know FFMPEG or a good understanding of streaming settings and are willing to help me optimize this, that would be huge. I want to make this a project for the community to benefit from and allow folks to stop paying outlandish prices for restream services. 

## License

This project is licensed under the GNU General Public License (GPL) Version 3. See the [LICENSE](LICENSE) file for details.

