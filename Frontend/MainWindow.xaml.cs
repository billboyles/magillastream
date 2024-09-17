using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utilities;
using Backend;
using System.IO;

namespace Frontend
{
    public partial class MainWindow : Window
    {
        private ProfileManager _profileManager;
        private FFmpegService _ffmpegService;
        private AppSettings _appSettings;
        
        private int outputGroupCount = 1; // Track how many output groups are present

        // List to hold references to OutputGroupControls
        private List<OutputGroupControls> outputGroupControlsList = new List<OutputGroupControls>();

        public MainWindow()
        {
            InitializeComponent();
            _profileManager = new ProfileManager();
            _ffmpegService = new FFmpegService();
            _appSettings = new AppSettings();

            // Load AppSettings
            _appSettings = AppSettings.Load();

            // Load last used profile if it exists
            if (!string.IsNullOrEmpty(_appSettings.LastUsedProfile))
            {
                try
                {
                    Profile lastProfile = _profileManager.LoadProfile(_appSettings.LastUsedProfile);
                    ApplyProfileToGUI(lastProfile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading last used profile: {ex.Message}");
                }
            }

            // Set maximum window size based on screen size
            this.MaxWidth = SystemParameters.WorkArea.Width;
            this.MaxHeight = SystemParameters.WorkArea.Height;

            // Attach event handlers
            createProfileButton.Click -= CreateProfileButton_Click;
            createProfileButton.Click += CreateProfileButton_Click;

            saveProfileButton.Click -= SaveProfileButton_Click;
            saveProfileButton.Click += SaveProfileButton_Click;

            loadProfileButton.Click -= LoadProfileButton_Click;
            loadProfileButton.Click += LoadProfileButton_Click;

            deleteProfileButton.Click -= DeleteProfileButton_Click;
            deleteProfileButton.Click += DeleteProfileButton_Click;

            startStreamButton.Click -= StartStreamButton_Click;
            startStreamButton.Click += StartStreamButton_Click;

            stopStreamButton.Click -= StopStreamButton_Click;
            stopStreamButton.Click += StopStreamButton_Click;

            addOutputGroupButton.Click -= AddOutputGroupButton_Click;
            addOutputGroupButton.Click += AddOutputGroupButton_Click;
        }

        // Save the settings when closing the app
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            _appSettings.Save(); // Save the settings when closing the app
        }

        // Method to load a profile and apply the settings to the GUI
        private void ApplyProfileToGUI(Profile profile)
        {
            // Set the incoming URL and Generate PTS option
            IncomingUrlTextBox.Text = profile.IncomingUrl;
            GeneratePTSCheckBox.IsChecked = profile.GeneratePTS;

            // Clear existing output groups in the GUI and control list
            OutputGroupStack.Children.Clear();
            outputGroupControlsList.Clear();
            outputGroupCount = 1;

            // Load output groups from the profile
            foreach (var group in profile.OutputGroups)
            {
                AddOutputGroupToGUI(group); // This method will populate the fields in the GUI for each output group
            }
        }

        // Method to add an output group to the GUI
        private void AddOutputGroupToGUI(OutputGroup group)
        {
            GroupBox newGroup = new GroupBox
            {
                Header = $"Output Group {outputGroupCount++}",
                Style = (Style)FindResource("GroupBoxStyle")
            };

            StackPanel groupStackPanel = new StackPanel();

            // Create encoding settings panel
            StackPanel encodingSettingsPanel = new StackPanel
            {
                Visibility = group.ForwardOriginal ? Visibility.Collapsed : Visibility.Visible
            };

            // Create and store references to controls
            ComboBox videoEncoderComboBox = CreateComboBox(new string[] { "libx264", "libx265" });
            videoEncoderComboBox.SelectedItem = GetComboBoxItemByContent(videoEncoderComboBox, group.EncodingSettings?.VideoEncoder);

            TextBox resolutionTextBox = CreateTextBox(group.EncodingSettings?.Resolution ?? "1920x1080");
            TextBox fpsTextBox = CreateTextBox(group.EncodingSettings?.Fps.ToString() ?? "30");
            TextBox videoBitrateTextBox = CreateTextBox(group.EncodingSettings?.Bitrate ?? "6000k");
            ComboBox audioCodecComboBox = CreateComboBox(new string[] { "aac", "mp3" });
            audioCodecComboBox.SelectedItem = GetComboBoxItemByContent(audioCodecComboBox, group.EncodingSettings?.AudioCodec);
            TextBox audioBitrateTextBox = CreateTextBox(group.EncodingSettings?.AudioBitrate ?? "192k");

            // Add controls to encoding settings panel
            encodingSettingsPanel.Children.Add(CreateTextBlock("Video Encoder"));
            encodingSettingsPanel.Children.Add(videoEncoderComboBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Resolution"));
            encodingSettingsPanel.Children.Add(resolutionTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("FPS"));
            encodingSettingsPanel.Children.Add(fpsTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Bitrate (Video)"));
            encodingSettingsPanel.Children.Add(videoBitrateTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Codec"));
            encodingSettingsPanel.Children.Add(audioCodecComboBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Bitrate"));
            encodingSettingsPanel.Children.Add(audioBitrateTextBox);

            // Create Forward Original Stream checkbox
            CheckBox forwardOriginalCheckBox = new CheckBox
            {
                Content = "Forward Original Stream",
                IsChecked = group.ForwardOriginal,
                Margin = new Thickness(0, 0, 0, 10),
                Tag = encodingSettingsPanel
            };
            forwardOriginalCheckBox.Checked += ForwardOriginalCheckBox_CheckedChanged;
            forwardOriginalCheckBox.Unchecked += ForwardOriginalCheckBox_CheckedChanged;

            groupStackPanel.Children.Add(forwardOriginalCheckBox);
            groupStackPanel.Children.Add(encodingSettingsPanel);

            // Add a Separator (Horizontal rule)
            Separator separator = new Separator
            {
                Margin = new Thickness(0, 5, 0, 10),
                Background = Brushes.Gray
            };
            groupStackPanel.Children.Add(separator);

            // Add Output URLs section title
            TextBlock outputUrlsTitle = CreateTextBlock("Output URLs");
            outputUrlsTitle.Margin = new Thickness(0, 10, 0, 5); // Adjust margin for title
            groupStackPanel.Children.Add(outputUrlsTitle);

            // Create OutputGroupControls and store references
            var outputGroupControls = new OutputGroupControls
            {
                GroupBox = newGroup,
                ForwardOriginalCheckBox = forwardOriginalCheckBox,
                EncodingSettingsPanel = encodingSettingsPanel,
                VideoEncoderComboBox = videoEncoderComboBox,
                ResolutionTextBox = resolutionTextBox,
                FpsTextBox = fpsTextBox,
                VideoBitrateTextBox = videoBitrateTextBox,
                AudioCodecComboBox = audioCodecComboBox,
                AudioBitrateTextBox = audioBitrateTextBox,
                OutputUrlControlsList = new List<OutputUrlControls>()
            };

            foreach (var outputUrl in group.OutputUrls)
            {
                StackPanel urlPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 10, 0, 10)
                };

                TextBox outputUrlTextBox = CreateTextBox(outputUrl.Url);
                TextBox streamKeyTextBox = CreateTextBox(outputUrl.StreamKey);
                ComboBox platformTemplateComboBox = CreateComboBox(new string[] { "Twitch", "YouTube", "Custom" });
                platformTemplateComboBox.SelectedItem = GetComboBoxItemByContent(platformTemplateComboBox, outputUrl.Template);

                urlPanel.Children.Add(CreateTextBlock("Output URL"));
                urlPanel.Children.Add(outputUrlTextBox);
                urlPanel.Children.Add(CreateTextBlock("Stream Key"));
                urlPanel.Children.Add(streamKeyTextBox);
                urlPanel.Children.Add(CreateTextBlock("Platform Template"));
                urlPanel.Children.Add(platformTemplateComboBox);

                groupStackPanel.Children.Add(urlPanel);

                var outputUrlControls = new OutputUrlControls
                {
                    OutputUrlTextBox = outputUrlTextBox,
                    StreamKeyTextBox = streamKeyTextBox,
                    PlatformTemplateComboBox = platformTemplateComboBox
                };

                outputGroupControls.OutputUrlControlsList.Add(outputUrlControls);
            }

            // Add Output Url button
            Button addOutputUrlButton = new Button
            {
                Style = (Style)FindResource("AddButtonStyle"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            addOutputUrlButton.Click += AddOutputUrlButton_Click;

            groupStackPanel.Children.Add(addOutputUrlButton);

            newGroup.Content = groupStackPanel;
            OutputGroupStack.Children.Add(newGroup);

            outputGroupControlsList.Add(outputGroupControls);
        }

        // Helper method to select ComboBoxItem by content
        private ComboBoxItem GetComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == content)
                {
                    return item;
                }
            }
            return null;
        }

        // Event handler for creating a new profile
        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            CreateProfileDialog dialog = new CreateProfileDialog();
            dialog.ShowDialog();
        }

        // Event handler for loading a profile
        private void LoadProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the list of profiles from ProfileManager
            List<string> profiles = _profileManager.GetProfilesList();

            // Open the custom profile selection dialog
            LoadProfileDialog profileDialog = new LoadProfileDialog(profiles);
            
            if (profileDialog.ShowDialog() == true)
            {
                // Load the selected profile
                string selectedProfile = profileDialog.SelectedProfile;

                try
                {
                    Profile loadedProfile = _profileManager.LoadProfile(selectedProfile);
                    ApplyProfileToGUI(loadedProfile);

                    // Update the last opened profile
                    _appSettings.LastUsedProfile = selectedProfile;
                    _appSettings.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading profile: {ex.Message}");
                }
            }
        }

        // Event handler for deleting a new profile
        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteProfileDialog dialog = new DeleteProfileDialog();
            dialog.ShowDialog();
        }

        // Event handler for saving a profile
        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse the GUI into a Profile object
                var profile = ParseGUIToProfile();

                if (string.IsNullOrEmpty(_appSettings.LastUsedProfile))
                {
                    MessageBox.Show("No profile is loaded. Please load or create a profile before saving.");
                    return;
                }

                else
                {
                    profile.ProfileName = _appSettings.LastUsedProfile;

                    // Save the profile using the ProfileManager
                    _profileManager.SaveProfile(profile.ProfileName, profile);
                    MessageBox.Show("Profile saved successfully!");
                }
            }
            
            catch (Exception ex)
            {
                // Show an error message if something goes wrong
                MessageBox.Show($"Error saving profile: {ex.Message}");
            }
        }

        // Event handler for adding a new Output Group dynamically
        private void AddOutputGroupButton_Click(object sender, RoutedEventArgs e)
        {
            GroupBox newGroup = new GroupBox
            {
                Header = $"Output Group {outputGroupCount++}",
                Style = (Style)FindResource("GroupBoxStyle")
            };

            StackPanel groupStackPanel = new StackPanel();

            // Create encoding settings panel
            StackPanel encodingSettingsPanel = new StackPanel
            {
                Visibility = Visibility.Visible
            };

            // Create and store references to controls
            ComboBox videoEncoderComboBox = CreateComboBox(new string[] { "libx264", "libx265" });
            TextBox resolutionTextBox = CreateTextBox("1920x1080");
            TextBox fpsTextBox = CreateTextBox("30");
            TextBox videoBitrateTextBox = CreateTextBox("6000k");
            ComboBox audioCodecComboBox = CreateComboBox(new string[] { "aac", "mp3" });
            TextBox audioBitrateTextBox = CreateTextBox("192k");

            // Add controls to encoding settings panel
            encodingSettingsPanel.Children.Add(CreateTextBlock("Video Encoder"));
            encodingSettingsPanel.Children.Add(videoEncoderComboBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Resolution"));
            encodingSettingsPanel.Children.Add(resolutionTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("FPS"));
            encodingSettingsPanel.Children.Add(fpsTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Bitrate (Video)"));
            encodingSettingsPanel.Children.Add(videoBitrateTextBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Codec"));
            encodingSettingsPanel.Children.Add(audioCodecComboBox);
            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Bitrate"));
            encodingSettingsPanel.Children.Add(audioBitrateTextBox);

            // Create Forward Original Stream checkbox and assign the encoding panel to its Tag
            CheckBox forwardOriginalCheckBox = new CheckBox
            {
                Content = "Forward Original Stream",
                Margin = new Thickness(0, 0, 0, 10),
                Tag = encodingSettingsPanel // Store a reference to the encoding settings panel in the Tag property
            };

            forwardOriginalCheckBox.Checked += ForwardOriginalCheckBox_CheckedChanged;
            forwardOriginalCheckBox.Unchecked += ForwardOriginalCheckBox_CheckedChanged;

            groupStackPanel.Children.Add(forwardOriginalCheckBox);
            groupStackPanel.Children.Add(encodingSettingsPanel);

            // Add a Separator (Horizontal rule)
            Separator separator = new Separator
            {
                Margin = new Thickness(0, 5, 0, 10),
                Background = Brushes.Gray
            };
            groupStackPanel.Children.Add(separator);

            // Add Output URLs section title
            TextBlock outputUrlsTitle = CreateTextBlock("Output URLs");
            outputUrlsTitle.Margin = new Thickness(0, 10, 0, 5); // Adjust margin for title
            groupStackPanel.Children.Add(outputUrlsTitle);

            // Add Output Url button
            Button addOutputUrlButton = new Button
            {
                Style = (Style)FindResource("AddButtonStyle"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            addOutputUrlButton.Click += AddOutputUrlButton_Click;

            groupStackPanel.Children.Add(addOutputUrlButton);

            newGroup.Content = groupStackPanel;
            OutputGroupStack.Children.Add(newGroup);

            // Create OutputGroupControls and store references
            var outputGroupControls = new OutputGroupControls
            {
                GroupBox = newGroup,
                ForwardOriginalCheckBox = forwardOriginalCheckBox,
                EncodingSettingsPanel = encodingSettingsPanel,
                VideoEncoderComboBox = videoEncoderComboBox,
                ResolutionTextBox = resolutionTextBox,
                FpsTextBox = fpsTextBox,
                VideoBitrateTextBox = videoBitrateTextBox,
                AudioCodecComboBox = audioCodecComboBox,
                AudioBitrateTextBox = audioBitrateTextBox,
                OutputUrlControlsList = new List<OutputUrlControls>()
            };

            outputGroupControlsList.Add(outputGroupControls);
        }

        // Event handler for adding a new Output Url dynamically
        private void AddOutputUrlButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StackPanel groupPanel = button?.Parent as StackPanel;

            StackPanel urlPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(0, 10, 0, 10)
            };

            TextBox outputUrlTextBox = CreateTextBox("rtmp://your-url");
            TextBox streamKeyTextBox = CreateTextBox("Your Stream Key");
            ComboBox platformTemplateComboBox = CreateComboBox(new string[] { "Twitch", "YouTube", "Custom" });

            urlPanel.Children.Add(CreateTextBlock("Output URL"));
            urlPanel.Children.Add(outputUrlTextBox);
            urlPanel.Children.Add(CreateTextBlock("Stream Key"));
            urlPanel.Children.Add(streamKeyTextBox);
            urlPanel.Children.Add(CreateTextBlock("Platform Template"));
            urlPanel.Children.Add(platformTemplateComboBox);

            // Insert urlPanel before the Add Output Url button
            int insertIndex = groupPanel.Children.IndexOf(button);
            groupPanel.Children.Insert(insertIndex, urlPanel);

            // Find the corresponding OutputGroupControls
            var outputGroupControls = outputGroupControlsList.Find(ogc => ogc.GroupBox.Content == groupPanel);

            if (outputGroupControls != null)
            {
                var outputUrlControls = new OutputUrlControls
                {
                    OutputUrlTextBox = outputUrlTextBox,
                    StreamKeyTextBox = streamKeyTextBox,
                    PlatformTemplateComboBox = platformTemplateComboBox
                };

                outputGroupControls.OutputUrlControlsList.Add(outputUrlControls);
            }
        }

        // Toggle the visibility of encoding settings based on the ForwardOriginal checkbox
        private void ForwardOriginalCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            var encodingSettingsPanel = checkbox?.Tag as StackPanel; // Get the linked encoding settings panel via the Tag

            if (encodingSettingsPanel != null)
            {
                // Hide or show encoding settings based on the checkbox state
                if (checkbox.IsChecked == true)
                {
                    encodingSettingsPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    encodingSettingsPanel.Visibility = Visibility.Visible;
                }
            }
        }

        // Helper method to create TextBlocks
        private TextBlock CreateTextBlock(string text)
        {
            return new TextBlock
            {
                Text = text,
                Margin = new Thickness(0, 0, 0, 5),
                FontWeight = FontWeights.Bold
            };
        }

        // Helper method to create TextBoxes
        private TextBox CreateTextBox(string defaultValue)
        {
            return new TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        // Helper method to create ComboBoxes
        private ComboBox CreateComboBox(string[] items)
        {
            ComboBox comboBox = new ComboBox
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            foreach (var item in items)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = item });
            }

            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        // Event handler for starting the stream
        private void StartStreamButton_Click(object sender, RoutedEventArgs e)
        {
            // Parse the GUI into a Profile object
            var profile = ParseGUIToProfile();

            try
            {
                _ffmpegService.StartFFmpegProcess(profile);
                MessageBox.Show($"Streaming started.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting stream: {ex.Message}");
            }
        }

        // Event handler for stopping the stream
        private void StopStreamButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call the FFmpegService to stop all running FFmpeg processes
                _ffmpegService.StopFFmpegProcess();

                MessageBox.Show("All streams have been stopped.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping stream: {ex.Message}");
            }
        }

        private Profile ParseGUIToProfile()
        {
            Profile profile = new Profile
            {
                IncomingUrl = IncomingUrlTextBox.Text,
                GeneratePTS = GeneratePTSCheckBox.IsChecked == true,
                OutputGroups = new List<OutputGroup>()
            };

            foreach (var ogControls in outputGroupControlsList)
            {
                OutputGroup outputGroup = new OutputGroup
                {
                    ForwardOriginal = ogControls.ForwardOriginalCheckBox.IsChecked == true,
                    OutputUrls = new List<OutputUrl>()
                };

                if (!outputGroup.ForwardOriginal)
                {
                    outputGroup.EncodingSettings = new Settings
                    {
                        VideoEncoder = (ogControls.VideoEncoderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        Resolution = ogControls.ResolutionTextBox.Text,
                        Fps = int.TryParse(ogControls.FpsTextBox.Text, out int fpsValue) ? fpsValue : 30,
                        Bitrate = ogControls.VideoBitrateTextBox.Text,
                        AudioCodec = (ogControls.AudioCodecComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                        AudioBitrate = ogControls.AudioBitrateTextBox.Text
                    };
                }

                foreach (var urlControls in ogControls.OutputUrlControlsList)
                {
                    OutputUrl outputUrl = new OutputUrl
                    {
                        Url = urlControls.OutputUrlTextBox.Text,
                        StreamKey = urlControls.StreamKeyTextBox.Text,
                        Template = (urlControls.PlatformTemplateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                    };
                    outputGroup.OutputUrls.Add(outputUrl);
                }

                profile.OutputGroups.Add(outputGroup);
            }

            return profile;
        }
    }
}
