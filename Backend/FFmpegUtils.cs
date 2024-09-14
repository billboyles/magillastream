using System.Collections.Generic;
using System.Diagnostics;
using Utilities;

public static class FFmpegUtils
{
    // Example method to get a list of supported encoders from FFmpeg
    public static List<string> GetAvailableEncoders()
    {
        List<string> encoders = new List<string>();
        Process ffmpeg = new Process();

        // Command to get the list of FFmpeg encoders
        ffmpeg.StartInfo.FileName = "ffmpeg";
        ffmpeg.StartInfo.Arguments = "-encoders";
        ffmpeg.StartInfo.RedirectStandardOutput = true;
        ffmpeg.StartInfo.UseShellExecute = false;
        ffmpeg.StartInfo.CreateNoWindow = true;

        try
        {
            // Try starting the process
            if (ffmpeg.Start())
            {
                // Read the output from the process
                while (!ffmpeg.StandardOutput.EndOfStream)
                {
                    string? line = ffmpeg.StandardOutput.ReadLine();  // Use nullable string

                    // Check if the line is not null and contains "Encoder"
                    if (!string.IsNullOrEmpty(line) && line.Contains("Encoder"))
                    {
                        encoders.Add(line);  // Add the valid line to the encoders list
                    }
                }

                ffmpeg.WaitForExit();
            }
            else
            {
                // Handle the case where the process couldn't start
                throw new System.Exception("FFmpeg process failed to start.");
            }
        }
        catch (System.Exception ex)
        {
            // Log the exception or handle it accordingly
            Console.WriteLine($"Error: {ex.Message}");
        }

        return encoders;
    }
}
