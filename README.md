# MagillaStream

## Overview

MagillaStream is a lightweight and portable RTMP server application that enables streamers to broadcast to YouTube and Twitch with customizable settings. It features a GUI for configuration and manages re-encoding as needed.

## Getting Started

You can use MagillaStream several ways - by using the prebuilt EXE file, by building your own EXE from the source code, or by running the source code using Python.

## Using the EXE

1. Download the ZIP file [MagillaStream.zip](https://github.com/billboyles/magillastream/blob/main/MagillaStream.zip) containing the app EXE and other necessary components.

2. Unzip the file in a desired location, such as your home folder.

3. Run the EXE (you may want to add a shortcut to your desktop). When you first run the application, Windows might show a security warning because the app is unrecognized. You can safely allow it to run by choosing "More info" and then "Run anyway".

4. Windows may block some files after download. To resolve this, you can use PowerShell as an Administrator and run `Get-ChildItem -Path "Your\UnzipPath" -Recurse | Unblock-File`. Alternatively, right-click on `nginx.exe`, go to Properties, and check "Unblock". This may avoid the above security warning.

5. The first time you click "Start Services" in MagillaStream, you might need to allow `nginx.exe` through your Windows Firewall to enable streaming functionality.

   *If you are uncomfortable with allowing an untrusted app to run, consider building the app from source or running it using Python as detailed below. You can also supply your own binaries for Nginx and FFmpeg - just be sure to use compatible versions. Nginx needs to have the RTMP module enabled, and FFMPEG needs to have the correct options for your specific hardware. Both binaries need to be compiled for Windows.*

No additional installations or dependencies are needed when running the app from the supplied EXE.

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

`pyinstaller --onefile --add-data "ffmpeg;ffmpeg" --add-data "nginx;nginx" main.py`

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

## Support and Improvements

Please use Github to open an issue if you have questions, need support, encounter a bug, or have a suggestion for a new feature or enhancement.

If you would like to help develop and improve the app, please reach out.

## Acknowledgements and Licenses

This simp project is dedicated to my favorite streamer, **Spirt Art Life**. You should check her out on [Youtube](https://www.youtube.com/@SpiritArtLife) or [Twitch](https://www.twitch.tv/spiritartlife) and give her a follow. It's the best way to support this project.

Thank you to the following streamers for testing the app and providing early feedback:
- Spirt Art Life | [Youtube](https://www.youtube.com/@SpiritArtLife) | [Twitch](https://www.twitch.tv/spiritartlife)
- Spanj | [Youtube](https://www.youtube.com/@Spanj) | [Twitch](https://www.twitch.tv/spanj)
- Johnny Jay Appleseed | [Youtube](https://www.youtube.com/@JohnnyJayAppleseed) | [Twitch](https://www.twitch.tv/johnnyjayappleseed)
- Nerdlette Gaming | [Youtube](https://www.youtube.com/@NerdletteGaming) | [Twitch](https://www.twitch.tv/nerdlettegaming)
- Ironwulf | [Youtube](https://www.youtube.com/@Ironwulf007) | [Twitch](https://www.twitch.tv/ironwulf007)

Please check them out and give them a follow!

This project uses open-source components:

- **FFmpeg**, licensed under the LGPL/GPL. More information and source code can be found at [FFmpeg's website](https://ffmpeg.org).
- **Nginx**, which is licensed under a BSD-like license. More details and source code are available on [Nginx's website](https://nginx.org).

I am grateful to the developers of these tools for their contributions to the open-source community.

This project is licensed under the GNU General Public License (GPL) Version 3. See the [LICENSE](LICENSE.txt) file for details.
