using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Utilities;
using Backend;

namespace Frontend
{
    public partial class MainWindow : Window
    {
        private ProfileManager _profileManager;
        private FFmpegService _ffmpegService;
        private int outputGroupCount = 1; // Track how many output groups are present

        public MainWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _ffmpegService = new FFmpegService();

            // Attach event handlers using +=
            createProfileButton.Click += CreateProfileButton_Click;
            startStreamButton.Click += StartStreamButton_Click;
            stopStreamButton.Click += StopStreamButton_Click;
            addOutputGroupButton.Click += AddOutputGroup_Click;

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

        // Event handler for adding a new Output URL (this will be implemented later)
        private void AddOutputUrl_Click(object sender, RoutedEventArgs e)
        {
            // Logic for adding an Output URL goes here
            MessageBox.Show("Add Output URL clicked.");
        }

        // Event handler for adding a new Output Group dynamically
        private void AddOutputGroup_Click(object sender, RoutedEventArgs e)
        {
            // Increment the output group count
            outputGroupCount++;

            // Create a new Output Group dynamically
            GroupBox newGroup = new GroupBox
            {
                Header = $"Output Group {outputGroupCount}",
                Style = (Style)FindResource("GroupBoxStyle")
            };

            StackPanel groupStackPanel = new StackPanel();

            TextBlock encodingSettings = new TextBlock
            {
                Text = "Encoding Settings Area",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            TextBlock outputUrl = new TextBlock
            {
                Text = "Output URL Area",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            Button addOutputUrl = new Button
            {
                Style = (Style)FindResource("AddButtonStyle"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Attach event handler to the new "Add Output URL" button
            addOutputUrl.Click += AddOutputUrl_Click;

            groupStackPanel.Children.Add(encodingSettings);
            groupStackPanel.Children.Add(outputUrl);
            groupStackPanel.Children.Add(addOutputUrl);

            newGroup.Content = groupStackPanel;

            // Add the new output group to the OutputGroupStack
            OutputGroupStack.Children.Add(newGroup);
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
