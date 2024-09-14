using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Backend;
using Backend.Utilities;
using GUI.ViewModels;

namespace GUI
{
    public partial class MainWindow : Window
    {
        private SettingsManager? _settingsManager;
        private string _userPassword = string.Empty;
        private FFmpegService _ffmpegService = new FFmpegService();
        private string _profileName = string.Empty;
        private string _settingsFilePath;
        private int encodingColumnCount = 1;

        public MainWindow()
        {
            InitializeComponent();
            Logger.LogInfo("MainWindow initialized.");
            DataContext = new MainViewModel();

            // Handle profile selection
            var profileDialog = new ProfileDialog();
            if (profileDialog.ShowDialog() == true)
            {
                Logger.LogInfo("Profile dialog completed successfully.");
                _profileName = profileDialog.SelectedProfile;
                _settingsFilePath = $"profiles/settings_{_profileName}.enc";

                if (!File.Exists(_settingsFilePath))
                {
                    Logger.LogInfo("Creating new profile settings.");
                    PopulateDefaultDropdowns();
                    var settings = new AppSettings
                    {
                        Name = "DefaultProfile",
                        Encoder = "libx264",  // Set a default encoder
                        Resolution = "1080p", // Set a default resolution
                        Bitrate = "6000k",    // Set a default bitrate
                        OutputServices = new System.Collections.Generic.List<OutputService>() // Initialize empty list of output services
                    };
                    _settingsManager = new SettingsManager("default-password", _profileName);
                    _settingsManager.SaveSettings(settings);
                }
                else
                {
                    Logger.LogInfo("Loading existing profile.");
                    var passwordDialog = new PasswordDialog();
                    if (passwordDialog.ShowDialog() == true)
                    {
                        _userPassword = passwordDialog.UserPassword;
                        _settingsManager = new SettingsManager(_userPassword, _profileName);
                        LoadSettings();
                    }
                    else
                    {
                        Logger.LogWarning("Password dialog canceled by user.");
                        MessageBox.Show("Password is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            else
            {
                Logger.LogWarning("Profile dialog canceled by user.");
                MessageBox.Show("Profile selection is required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadSettings()
        {
            Logger.LogInfo("Loading settings.");
            if (_settingsManager == null)
            {
                Logger.LogError("SettingsManager is null. Cannot load settings.");
                return;
            }

            var settings = _settingsManager.LoadSettings();
            ObsStreamUrl.Text = settings.ObsStreamUrl;

            // Load original stream outputs
            foreach (var output in settings.OriginalStreamOutputs)
            {
                AddOutputUrlColumn(output.Url, output.StreamKey);
            }

            // Load encoding columns
            encodingColumnCount = settings.Encodings.Count + 1;
            foreach (var encoding in settings.Encodings)
            {
                AddEncodingColumn(encoding);
            }
        }

        private void PopulateDefaultDropdowns()
        {
            Logger.LogInfo("Populating default dropdowns.");
            var encoders = _ffmpegService.GetAvailableEncoders();
            if (encoders.Count > 0)
            {
                Logger.LogDebug("Encoders found: " + string.Join(", ", encoders));
            }
            else
            {
                Logger.LogWarning("No encoders found.");
            }
        }

        private void SaveSettings()
        {
            Logger.LogInfo("Saving settings.");
            if (_settingsManager == null)
            {
                Logger.LogError("SettingsManager is null. Cannot save settings.");
                return;
            }

            var encodingSettingsList = new System.Collections.Generic.List<EncodingSettings>();

            foreach (StackPanel column in EncodingColumnsGrid.Children)
            {
                var name = ((TextBox)column.Children[1]).Text;
                var bitrate = ((TextBox)column.Children[3]).Text;
                var encoder = ((ComboBox)column.Children[5]).SelectedItem.ToString();
                var resolution = ((ComboBox)column.Children[7]).SelectedItem.ToString();

                encodingSettingsList.Add(new EncodingSettings
                {
                    Name = name,
                    Bitrate = bitrate,
                    Encoder = encoder,
                    Resolution = resolution,
                    OutputServices = new System.Collections.Generic.List<OutputService>()
                });
            }

            var settings = new AppSettings
            {
                Name = "UpdatedProfile",  // Set the profile name
                ObsStreamUrl = ObsStreamUrl.Text, // Use the stream URL from the user input
                Encoder = "libx264",  // This could be set dynamically, but for now, we'll use a default value
                Resolution = "1080p", // This could also be dynamic, for now, we use a default value
                Bitrate = "6000k",    // Default bitrate or user-defined
                OutputServices = new System.Collections.Generic.List<OutputService>(), // Initialize with empty list
                Encodings = encodingSettingsList  // Set the list of encodings (from the UI)
            };

            _settingsManager.SaveSettings(settings);
        }

        private void AddEncodingColumn(EncodingSettings encoding)
        {
            Logger.LogInfo($"Adding encoding column: {encoding.Name}");
            StackPanel encodingColumn = new StackPanel { Margin = new Thickness(10) };

            encodingColumn.Children.Add(new TextBlock { Text = "Name", Margin = new Thickness(0, 10, 0, 0) });
            encodingColumn.Children.Add(new TextBox { Text = encoding.Name ?? $"Column{encodingColumnCount}", Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            encodingColumn.Children.Add(new TextBlock { Text = "Bitrate", Margin = new Thickness(0, 10, 0, 0) });
            encodingColumn.Children.Add(new TextBox { Text = encoding.Bitrate, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            encodingColumn.Children.Add(new TextBlock { Text = "Encoder", Margin = new Thickness(0, 10, 0, 0) });
            ComboBox encoderDropdown = new ComboBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) };
            var encoders = _ffmpegService.GetAvailableEncoders();
            encoderDropdown.ItemsSource = encoders.Count > 0 ? encoders.ToArray() : new[] { "libx264", "h264_qsv", "h264_nvenc" };
            encoderDropdown.SelectedItem = encoding.Encoder;
            encodingColumn.Children.Add(encoderDropdown);

            encodingColumn.Children.Add(new TextBlock { Text = "Resolution", Margin = new Thickness(0, 10, 0, 0) });
            ComboBox resolutionDropdown = new ComboBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) };
            resolutionDropdown.ItemsSource = new[] { "360p", "480p", "720p", "1080p", "1440p", "4K" };
            resolutionDropdown.SelectedItem = encoding.Resolution;
            encodingColumn.Children.Add(resolutionDropdown);

            Button addUrlButton = new Button { Content = "Add URL", Width = 100, Height = 30, Margin = new Thickness(0, 10, 0, 0) };
            addUrlButton.Click += AddUrl_Click;
            encodingColumn.Children.Add(addUrlButton);

            EncodingColumnsGrid.Children.Add(encodingColumn);
            encodingColumnCount++;
        }

        private void AddUrl_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Adding URL.");
            Button addUrlButton = sender as Button;
            StackPanel parentPanel = addUrlButton?.Parent as StackPanel;

            if (parentPanel != null)
            {
                parentPanel.Children.Add(new TextBlock { Text = "Stream URL", Margin = new Thickness(0, 10, 0, 0) });
                parentPanel.Children.Add(new TextBox { Height = 30, Width = 200, Margin = new Thickness(0, 0, 0, 10) });
            }
        }

        private void AddOutputUrlColumn(string url, string streamKey)
        {
            Logger.LogInfo($"Adding output URL column: {url}");
            StackPanel urlColumn = new StackPanel { Margin = new Thickness(10) };

            urlColumn.Children.Add(new TextBlock { Text = "Stream URL", Margin = new Thickness(0, 10, 0, 0) });
            urlColumn.Children.Add(new TextBox { Text = url, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            urlColumn.Children.Add(new TextBlock { Text = "Stream Key", Margin = new Thickness(0, 10, 0, 0) });
            urlColumn.Children.Add(new TextBox { Text = streamKey, Height = 30, Margin = new Thickness(0, 0, 0, 10) });

            EncodingColumnsGrid.Children.Add(urlColumn);
        }

        // Handler for the 'Add Encoding' button
        private void AddEncodingColumn_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Add Encoding button clicked.");
            AddEncodingColumn(new EncodingSettings
            {
                Name = $"Column{encodingColumnCount}",
                Bitrate = "6000k",
                Encoder = "libx264",
                Resolution = "1080p",
                OutputServices = new System.Collections.Generic.List<OutputService>()
            });
        }

        // Handler for the 'Start' button
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Start button clicked. Preparing to start the stream.");

            string obsStreamUrl = ObsStreamUrl.Text;
            var outputServices = new System.Collections.Generic.List<Tuple<string, string, bool, string, string, string>>();

            foreach (StackPanel column in EncodingColumnsGrid.Children)
            {
                var urlTextBox = (TextBox)column.Children[1];
                var streamKeyTextBox = (TextBox)column.Children[3];
                var bitrateTextBox = (TextBox)column.Children[5];
                var resolutionComboBox = (ComboBox)column.Children[7];
                var reEncodeCheckBox = (CheckBox)column.Children[8];
                var encoderComboBox = (ComboBox)column.Children[10];

                var outputUrl = urlTextBox.Text;
                var streamKey = streamKeyTextBox.Text;
                var bitrate = bitrateTextBox.Text;
                var resolution = resolutionComboBox.SelectedItem?.ToString() ?? "1080p";
                var encoder = encoderComboBox.SelectedItem?.ToString() ?? "libx264";
                bool reEncode = reEncodeCheckBox.IsChecked ?? false;

                outputServices.Add(Tuple.Create(outputUrl, streamKey, reEncode, bitrate, resolution, encoder));
            }

            _ffmpegService.StartStream(obsStreamUrl, outputServices, false);

            Logger.LogInfo("Stream started.");
        }

        // Handler for the 'Stop' button
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Stop button clicked. Stopping the stream.");
            _ffmpegService.StopStream();
        }

        // Handler for the 'Save' button
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Save button clicked.");
            SaveSettings();
        }
    }
}
