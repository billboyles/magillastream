using System.IO;
using System.Windows;
using Backend;
using Backend.Utilities;
using GUI.ViewModels;

namespace GUI
{
    public partial class MainWindow : Window
    {
        private SettingsManager _settingsManager;
        private string _userPassword;
        private FFmpegService _ffmpegService;
        private string _settingsFilePath = "settings.enc";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            _ffmpegService = new FFmpegService(); 

            if (!File.Exists(_settingsFilePath))
            {
                PopulateDefaultDropdowns();
                var settings = new AppSettings
                {
                    ObsStreamUrl = "rtmp://your-default-url",
                    StreamKey = string.Empty,
                    SelectedService = "YouTube",
                    Bitrate = "6000",
                    SelectedResolution = "1080p",
                    SelectedEncoder = "libx264"
                };
                _settingsManager = new SettingsManager("default-password");
                _settingsManager.SaveSettings(settings);
            }
            else
            {
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

        private void LoadSettings()
        {
            var settings = _settingsManager.LoadSettings();
            ObsStreamUrl.Text = settings.ObsStreamUrl;
            ServiceDropdown.SelectedItem = settings.SelectedService;
            ResolutionDropdown.SelectedItem = settings.SelectedResolution;
            EncoderDropdown.SelectedItem = settings.SelectedEncoder;
            StreamKeyTextBox.Text = settings.StreamKey;
            BitrateTextBox.Text = settings.Bitrate;
        }

        private void PopulateDefaultDropdowns()
        {
            ServiceDropdown.ItemsSource = new[] { "YouTube", "Twitch" };
            ResolutionDropdown.ItemsSource = new[] { "360p", "480p", "720p", "1080p", "1440p", "4K" };

            // Get available encoders using FFmpeg
            var encoders = _ffmpegService.GetAvailableEncoders();
            EncoderDropdown.ItemsSource = encoders.Count > 0 ? encoders : new[] { "libx264", "h264_qsv", "h264_nvenc" };
        }

        private void SaveSettings()
        {
            var settings = new AppSettings
            {
                ObsStreamUrl = ObsStreamUrl.Text,
                SelectedService = ServiceDropdown.SelectedItem.ToString(),
                SelectedResolution = ResolutionDropdown.SelectedItem.ToString(),
                SelectedEncoder = EncoderDropdown.SelectedItem.ToString(),
                StreamKey = StreamKeyTextBox.Text,
                Bitrate = BitrateTextBox.Text
            };

            _settingsManager.SaveSettings(settings);
        }
    }
}
