using System;
using System.Collections.Generic;
using System.Windows;
using Utilities; // ProfileManager and Profile are in Utilities
using Backend;  // FFmpegService is in Backend

namespace Frontend
{
    public partial class MainWindow : Window
    {
        private ProfileManager _profileManager;
        private FFmpegService _ffmpegService;

        public MainWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _ffmpegService = new FFmpegService();

            // Load profiles into the ComboBox
            LoadProfiles();
        }

        // Event handler for creating a new profile
        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Prompt for profile name
            string profileName = Microsoft.VisualBasic.Interaction.InputBox("Enter Profile Name", "Create Profile");

            if (string.IsNullOrEmpty(profileName))
            {
                MessageBox.Show("Profile name cannot be empty.");
                return;
            }

            // Create an empty profile without output groups for now
            Profile profile = new Profile
            {
                ProfileName = profileName,
                IncomingUrl = IncomingUrlTextBox.Text,
                OutputGroups = new List<OutputGroup>(),  // Empty output groups for now
                GeneratePTS = GeneratePTSCheckBox.IsChecked ?? false,
                Theme = "light", // Default theme, this can be changed later
                Language = "en-US" // Default language, can be changed
            };

            try
            {
                // Save the profile using ProfileManager
                _profileManager.CreateProfile(profileName, profile);

                MessageBox.Show("Profile created successfully!");

                // Reload profiles to update the ComboBox
                LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}");
            }
        }

        // Event handler for starting the stream
        private void StartStreamButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedProfile = ProfileComboBox.SelectedItem?.ToString();

            // Check if a valid profile is selected
            if (string.IsNullOrEmpty(selectedProfile))
            {
                MessageBox.Show("Please select a valid profile to start streaming.");
                return;
            }

            try
            {
                // Load the selected profile
                Profile profile = _profileManager.LoadProfile(selectedProfile);

                // Start the FFmpeg process for the profile
                _ffmpegService.StartFFmpegProcess(profile);

                MessageBox.Show($"Streaming started with profile {selectedProfile}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting stream: {ex.Message}");
            }
        }

        // Event handler for stopping the stream
        private void StopStreamButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic to stop the streaming process (terminate the FFmpeg process)
            MessageBox.Show("Stream stopped.");
            // Example: If you started FFmpeg as a process, you would kill that process here
        }

        // Load profiles into the ComboBox
        private void LoadProfiles()
        {
            ProfileComboBox.Items.Clear();

            // Assuming profiles are stored in a directory and listed by file names
            string[] profileFiles = System.IO.Directory.GetFiles("profiles", "*.enc");

            foreach (string profileFile in profileFiles)
            {
                string profileName = System.IO.Path.GetFileNameWithoutExtension(profileFile);
                ProfileComboBox.Items.Add(profileName);
            }

            if (ProfileComboBox.Items.Count > 0)
            {
                ProfileComboBox.SelectedIndex = 0;  // Select the first profile by default
            }
        }
    }
}
