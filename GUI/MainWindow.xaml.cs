using System.IO;
using System.Windows;
using Backend;
using Backend.Utilities;
using GUI.ViewModels;
using System.Windows.Controls;
using System.Collections.Generic;

namespace GUI
{
    public partial class MainWindow : Window
    {
        private SettingsManager? _settingsManager; // Nullable SettingsManager
        private string _userPassword = string.Empty; // Initialize to avoid warnings
        private FFmpegService _ffmpegService = new FFmpegService(); // Initialize FFmpegService
        private string _settingsFilePath = "settings.enc";
        private int encodingColumnCount = 1; // Start with 1 for the first column

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Handle user authentication for settings
            if (!File.Exists(_settingsFilePath))
            {
                // First-time setup
                PopulateDefaultDropdowns();
                var settings = new AppSettings
                {
                    ObsStreamUrl = "rtmp://your-default-url",
                    OriginalStreamOutputs = new List<OutputService> {
                        new OutputService { Url = "rtmp://youtube-url", StreamKey = "default-key" }
                    },
                    Encodings = new List<EncodingSettings>
                    {
                        new EncodingSettings
                        {
                            Name = $"Column{encodingColumnCount}",
                            Encoder = "libx264",
                            Resolution = "1080p",
                            Bitrate = "6000k",
                            OutputServices = new List<OutputService> {
                                new OutputService { Url = "rtmp://twitch-url", StreamKey = "default-twitch-key" }
                            }
                        }
                    }
                };
                _settingsManager = new SettingsManager("default-password");
                _settingsManager.SaveSettings(settings);
            }
            else
            {
                // Load settings if the settings file exists
                var passwordDialog = new PasswordDialog();
                if (passwordDialog.ShowDialog() == true)
                {
                    _userPassword = passwordDialog.UserPassword;
                    _settingsManager = new SettingsManager(_userPassword);
                    LoadSettings();
                }
                else
                {
                    MessageBox.Show("Password is required to proceed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
        }

        // Load saved settings
        private void LoadSettings()
        {
            if (_settingsManager == null) return;

            var settings = _settingsManager.LoadSettings();
            ObsStreamUrl.Text = settings.ObsStreamUrl;

            // For original stream outputs
            foreach (var output in settings.OriginalStreamOutputs)
            {
                AddOutputUrlColumn(output.Url, output.StreamKey);
            }

            // For encodings
            encodingColumnCount = settings.Encodings.Count + 1; // Set the count based on saved encodings
            foreach (var encoding in settings.Encodings)
            {
                AddEncodingColumn(encoding);
            }
        }

        // Populate default values for dropdowns
        private void PopulateDefaultDropdowns()
        {
            // Example default encoders and settings
            var encoders = _ffmpegService.GetAvailableEncoders();
            var defaultEncoders = encoders.Count > 0 ? encoders.ToArray() : new[] { "libx264", "h264_qsv", "h264_nvenc" };
        }

        // Save current settings
        private void SaveSettings()
        {
            if (_settingsManager == null) return;

            // Gather encoding columns data
            List<EncodingSettings> encodingSettingsList = new List<EncodingSettings>();
            foreach (StackPanel column in EncodingColumnsGrid.Children)
            {
                var name = ((TextBox)column.Children[1]).Text; // Name field
                var bitrate = ((TextBox)column.Children[3]).Text; // Bitrate field
                var encoder = ((ComboBox)column.Children[5]).SelectedItem.ToString(); // Encoder field
                var resolution = ((ComboBox)column.Children[7]).SelectedItem.ToString(); // Resolution field

                encodingSettingsList.Add(new EncodingSettings
                {
                    Name = name,
                    Bitrate = bitrate,
                    Encoder = encoder,
                    Resolution = resolution,
                    OutputServices = new List<OutputService>() // Logic to gather output services goes here
                });
            }

            var settings = new AppSettings
            {
                ObsStreamUrl = ObsStreamUrl.Text,
                OriginalStreamOutputs = new List<OutputService>(), // Logic to gather URLs from UI
                Encodings = encodingSettingsList // Save gathered encodings
            };

            _settingsManager.SaveSettings(settings);
        }

        // Handle dynamic addition of encoding columns
        private void AddEncodingColumn(EncodingSettings encoding)
        {
            // Create a new StackPanel for each encoding column
            StackPanel encodingColumn = new StackPanel { Margin = new Thickness(10) };

            // Name field for the encoding column
            encodingColumn.Children.Add(new TextBlock { Text = "Name", Margin = new Thickness(0, 10, 0, 0) });
            encodingColumn.Children.Add(new TextBox { Text = encoding.Name ?? $"Column{encodingColumnCount}", Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            // Bitrate
            encodingColumn.Children.Add(new TextBlock { Text = "Bitrate", Margin = new Thickness(0, 10, 0, 0) });
            encodingColumn.Children.Add(new TextBox { Text = encoding.Bitrate, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            // Encoder Selection
            encodingColumn.Children.Add(new TextBlock { Text = "Encoder", Margin = new Thickness(0, 10, 0, 0) });
            ComboBox encoderDropdown = new ComboBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) };
            var encoders = _ffmpegService.GetAvailableEncoders();
            encoderDropdown.ItemsSource = encoders.Count > 0 ? encoders.ToArray() : new[] { "libx264", "h264_qsv", "h264_nvenc" };
            encoderDropdown.SelectedItem = encoding.Encoder;
            encodingColumn.Children.Add(encoderDropdown);

            // Resolution Selection
            encodingColumn.Children.Add(new TextBlock { Text = "Resolution", Margin = new Thickness(0, 10, 0, 0) });
            ComboBox resolutionDropdown = new ComboBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) };
            resolutionDropdown.ItemsSource = new[] { "360p", "480p", "720p", "1080p", "1440p", "4K" };
            resolutionDropdown.SelectedItem = encoding.Resolution;
            encodingColumn.Children.Add(resolutionDropdown);

            // Button to add more URLs
            Button addUrlButton = new Button { Content = "Add URL", Width = 100, Height = 30, Margin = new Thickness(0, 10, 0, 0) };
            addUrlButton.Click += AddUrl_Click; // Hook up the URL adding event
            encodingColumn.Children.Add(addUrlButton);

            // Add the encoding column to the UniformGrid
            EncodingColumnsGrid.Children.Add(encodingColumn);

            encodingColumnCount++; // Increment the column count for the next added column
        }

        private void AddEncodingColumn_Click(object sender, RoutedEventArgs e)
        {
            // Logic for adding a new encoding column
            AddEncodingColumn(new EncodingSettings
            {
                Name = $"Column{encodingColumnCount}", // Default name
                Encoder = "libx264", // Default encoder
                Resolution = "1080p", // Default resolution
                Bitrate = "6000k", // Default bitrate
                OutputServices = new List<OutputService> { new OutputService { Url = "", StreamKey = "" } }
            });
        }

        // Handle adding a new URL input to an encoding column
        private void AddUrl_Click(object sender, RoutedEventArgs e)
        {
            Button addUrlButton = sender as Button;
            StackPanel parentPanel = addUrlButton?.Parent as StackPanel;

            if (parentPanel != null)
            {
                parentPanel.Children.Add(new TextBlock { Text = "Stream URL", Margin = new Thickness(0, 10, 0, 0) });
                parentPanel.Children.Add(new TextBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) });
            }
        }

        // Add a column for output URLs
        private void AddOutputUrlColumn(string url, string streamKey)
        {
            StackPanel urlColumn = new StackPanel { Margin = new Thickness(10) };

            urlColumn.Children.Add(new TextBlock { Text = "Stream URL", Margin = new Thickness(0, 10, 0, 0) });
            urlColumn.Children.Add(new TextBox { Text = url, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            urlColumn.Children.Add(new TextBlock { Text = "Stream Key", Margin = new Thickness(0, 10, 0, 0) });
            urlColumn.Children.Add(new TextBox { Text = streamKey, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            EncodingColumnsGrid.Children.Add(urlColumn);
        }
    }
}
