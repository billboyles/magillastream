﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utilities;
using Backend;
using System.IO;
using System.Text.Json;

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

            // Set maximum window size based on screen size
            this.MaxWidth = SystemParameters.WorkArea.Width;
            this.MaxHeight = SystemParameters.WorkArea.Height;

            // Attach event handlers only once
            createProfileButton.Click -= CreateProfileButton_Click;
            createProfileButton.Click += CreateProfileButton_Click;

            startStreamButton.Click -= StartStreamButton_Click;
            startStreamButton.Click += StartStreamButton_Click;

            stopStreamButton.Click -= StopStreamButton_Click;
            stopStreamButton.Click += StopStreamButton_Click;

            addOutputGroupButton.Click -= AddOutputGroupButton_Click;
            addOutputGroupButton.Click += AddOutputGroupButton_Click;

            saveProfileButton.Click -= SaveProfileButton_Click;
            saveProfileButton.Click += SaveProfileButton_Click;

            // Load profiles into the ComboBox
            LoadProfiles();
        }

        // Method to load a profile and apply the settings to the GUI
        private void ApplyProfileToGUI(Profile profile)
        {
            // Set the incoming URL and Generate PTS option
            IncomingUrlTextBox.Text = profile.IncomingUrl;
            GeneratePTSCheckBox.IsChecked = profile.GeneratePTS;

            // Clear existing output groups in the GUI
            OutputGroupStack.Children.Clear();
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
                Name = "EncodingSettingsPanel",
                Visibility = group.ForwardOriginal ? Visibility.Collapsed : Visibility.Visible
            };

            // Add the encoding settings UI elements
            encodingSettingsPanel.Children.Add(CreateTextBlock("Video Encoder"));
            ComboBox videoEncoderComboBox = CreateComboBox(new string[] { "libx264", "libx265" });
            videoEncoderComboBox.SelectedItem = group.EncodingSettings?.VideoEncoder;
            encodingSettingsPanel.Children.Add(videoEncoderComboBox);

            encodingSettingsPanel.Children.Add(CreateTextBlock("Resolution"));
            TextBox resolutionTextBox = CreateTextBox(group.EncodingSettings?.Resolution ?? "1920x1080");
            encodingSettingsPanel.Children.Add(resolutionTextBox);

            encodingSettingsPanel.Children.Add(CreateTextBlock("FPS"));
            TextBox fpsTextBox = CreateTextBox(group.EncodingSettings?.Fps.ToString() ?? "30");
            encodingSettingsPanel.Children.Add(fpsTextBox);

            encodingSettingsPanel.Children.Add(CreateTextBlock("Bitrate (Video)"));
            TextBox videoBitrateTextBox = CreateTextBox(group.EncodingSettings?.Bitrate ?? "6000k");
            encodingSettingsPanel.Children.Add(videoBitrateTextBox);

            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Codec"));
            ComboBox audioCodecComboBox = CreateComboBox(new string[] { "aac", "mp3" });
            audioCodecComboBox.SelectedItem = group.EncodingSettings?.AudioCodec;
            encodingSettingsPanel.Children.Add(audioCodecComboBox);

            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Bitrate"));
            TextBox audioBitrateTextBox = CreateTextBox(group.EncodingSettings?.AudioBitrate ?? "192k");
            encodingSettingsPanel.Children.Add(audioBitrateTextBox);

            // Create Forward Original Stream checkbox
            CheckBox forwardOriginalCheckBox = new CheckBox
            {
                Content = "Forward Original Stream",
                IsChecked = group.ForwardOriginal,
                Margin = new Thickness(0, 0, 0, 10),
                Tag = encodingSettingsPanel // Store a reference to the encoding settings panel in the Tag property
            };
            forwardOriginalCheckBox.Checked += ForwardOriginalCheckBox_CheckedChanged;
            forwardOriginalCheckBox.Unchecked += ForwardOriginalCheckBox_CheckedChanged;

            // Add the elements to the group stack panel
            groupStackPanel.Children.Add(forwardOriginalCheckBox);
            groupStackPanel.Children.Add(encodingSettingsPanel);

            // Add a Separator (Horizontal rule)
            Separator separator = new Separator
            {
                Margin = new Thickness(0, 5, 0, 10),
                Background = Brushes.Gray
            };
            groupStackPanel.Children.Add(separator);

            // Add output URLs
            TextBlock outputUrlsTitle = CreateTextBlock("Output URLs");
            groupStackPanel.Children.Add(outputUrlsTitle);

            foreach (var outputUrl in group.OutputUrls)
            {
                StackPanel urlPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 10, 0, 10)
                };

                urlPanel.Children.Add(CreateTextBlock("Output URL"));
                TextBox outputUrlTextBox = CreateTextBox(outputUrl.Url);
                urlPanel.Children.Add(outputUrlTextBox);

                urlPanel.Children.Add(CreateTextBlock("Stream Key"));
                TextBox streamKeyTextBox = CreateTextBox(outputUrl.StreamKey);
                urlPanel.Children.Add(streamKeyTextBox);

                urlPanel.Children.Add(CreateTextBlock("Platform Template"));
                ComboBox platformTemplateComboBox = CreateComboBox(new string[] { "Twitch", "YouTube", "Custom" });
                platformTemplateComboBox.SelectedItem = outputUrl.Template;
                urlPanel.Children.Add(platformTemplateComboBox);

                groupStackPanel.Children.Add(urlPanel);
            }

            // Add the group panel to the GUI
            newGroup.Content = groupStackPanel;
            OutputGroupStack.Children.Add(newGroup);
        }

        // Event handler for creating a new profile
        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string profileName = Microsoft.VisualBasic.Interaction.InputBox("Enter Profile Name", "Create Profile");

            if (string.IsNullOrEmpty(profileName))
            {
                MessageBox.Show("Profile name cannot be empty.");
                return;
            }

            Profile profile = new Profile
            {
                ProfileName = profileName,
                IncomingUrl = IncomingUrlTextBox.Text,
                OutputGroups = new List<OutputGroup>(),
                GeneratePTS = GeneratePTSCheckBox.IsChecked ?? false,
                Theme = "light",
                Language = "en-US"
            };

            try
            {
                _profileManager.CreateProfile(profileName, profile);
                MessageBox.Show("Profile created successfully!");
                LoadProfiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}");
            }
        }

        private void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse the GUI into a Profile object
                var profile = ParseGUIToProfile();

                // Save the profile using the ProfileManager
                _profileManager.SaveProfile(profile.ProfileName, profile);
                MessageBox.Show("Profile saved successfully!");
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
            outputGroupCount++;

            GroupBox newGroup = new GroupBox
            {
                Header = $"Output Group {outputGroupCount}",
                Style = (Style)FindResource("GroupBoxStyle")
            };

            StackPanel groupStackPanel = new StackPanel();

            // Create encoding settings panel
            StackPanel encodingSettingsPanel = new StackPanel
            {
                Name = "EncodingSettingsPanel",
                Visibility = Visibility.Visible
            };

            // Add the encoding settings UI elements
            encodingSettingsPanel.Children.Add(CreateTextBlock("Video Encoder"));
            encodingSettingsPanel.Children.Add(CreateComboBox(new string[] { "libx264", "libx265" }));

            encodingSettingsPanel.Children.Add(CreateTextBlock("Resolution"));
            encodingSettingsPanel.Children.Add(CreateTextBox("1920x1080"));

            encodingSettingsPanel.Children.Add(CreateTextBlock("FPS"));
            encodingSettingsPanel.Children.Add(CreateTextBox("30"));

            encodingSettingsPanel.Children.Add(CreateTextBlock("Bitrate (Video)"));
            encodingSettingsPanel.Children.Add(CreateTextBox("6000k"));

            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Codec"));
            encodingSettingsPanel.Children.Add(CreateComboBox(new string[] { "aac", "mp3" }));

            encodingSettingsPanel.Children.Add(CreateTextBlock("Audio Bitrate"));
            encodingSettingsPanel.Children.Add(CreateTextBox("192k"));

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

            urlPanel.Children.Add(CreateTextBlock("Output URL"));
            urlPanel.Children.Add(CreateTextBox("rtmp://your-url"));

            urlPanel.Children.Add(CreateTextBlock("Stream Key"));
            urlPanel.Children.Add(CreateTextBox("Your Stream Key"));

            urlPanel.Children.Add(CreateTextBlock("Platform Template"));
            urlPanel.Children.Add(CreateComboBox(new string[] { "Twitch", "YouTube", "Custom" }));

            groupPanel?.Children.Insert(groupPanel.Children.Count - 1, urlPanel);
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

        // Load profiles into the ComboBox
        private void LoadProfiles()
        {
            ProfileComboBox.Items.Clear();
            string[] profileFiles = System.IO.Directory.GetFiles("profiles", "*.json");

            foreach (string profileFile in profileFiles)
            {
                string profileName = System.IO.Path.GetFileNameWithoutExtension(profileFile);
                ProfileComboBox.Items.Add(profileName);
            }

            if (ProfileComboBox.Items.Count > 0)
            {
                ProfileComboBox.SelectedIndex = 0;
            }
        }

        // Event handler for starting the stream
        private void StartStreamButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedProfile = ProfileComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedProfile))
            {
                MessageBox.Show("Please select a valid profile to start streaming.");
                return;
            }

            try
            {
                Profile profile = _profileManager.LoadProfile(selectedProfile);
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

            foreach (GroupBox groupBox in OutputGroupStack.Children)
            {
                StackPanel groupStack = groupBox.Content as StackPanel;
                if (groupStack != null)
                {
                    OutputGroup outputGroup = new OutputGroup
                    {
                        EncodingSettings = new Settings(),
                        OutputUrls = new List<OutputUrl>(),
                        ForwardOriginal = (groupStack.Children[0] as CheckBox)?.IsChecked == true
                    };

                    StackPanel encodingSettingsPanel = groupStack.FindName("EncodingSettingsPanel") as StackPanel;
                    if (encodingSettingsPanel != null)
                    {
                        outputGroup.EncodingSettings.VideoEncoder = (encodingSettingsPanel.Children[1] as ComboBox)?.SelectedItem?.ToString();
                        outputGroup.EncodingSettings.Resolution = (encodingSettingsPanel.Children[3] as TextBox)?.Text;
                        outputGroup.EncodingSettings.Fps = int.TryParse((encodingSettingsPanel.Children[5] as TextBox)?.Text, out int fpsValue) ? fpsValue : 30;
                        outputGroup.EncodingSettings.Bitrate = (encodingSettingsPanel.Children[7] as TextBox)?.Text;
                        outputGroup.EncodingSettings.AudioCodec = (encodingSettingsPanel.Children[9] as ComboBox)?.SelectedItem?.ToString();
                        outputGroup.EncodingSettings.AudioBitrate = (encodingSettingsPanel.Children[11] as TextBox)?.Text;
                    }

                    for (int i = 2; i < groupStack.Children.Count; i++)
                    {
                        if (groupStack.Children[i] is StackPanel urlPanel && urlPanel.Children.Count >= 6)
                        {
                            OutputUrl outputUrl = new OutputUrl
                            {
                                Url = (urlPanel.Children[1] as TextBox)?.Text,
                                StreamKey = (urlPanel.Children[3] as TextBox)?.Text,
                                Template = (urlPanel.Children[5] as ComboBox)?.SelectedItem?.ToString()
                            };
                            outputGroup.OutputUrls.Add(outputUrl);
                        }
                    }

                    profile.OutputGroups.Add(outputGroup);
                }
            }

            return profile;
        }
    }
}
