using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Threading.Tasks;  // For async/await and Task
using MagillaStream.Models;
using MagillaStream.Utilities;

namespace MagillaStream.Views
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<OutputGroup> OutputGroups { get; set; }

        private readonly ProfileManager _profileManager;
        private AppSettings _appSettings;

        // Flag to prevent multiple initializations
        private bool _isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();

            OutputGroups = new ObservableCollection<OutputGroup>();
            _profileManager = new ProfileManager();
            _appSettings = LoadAppSettings();

            DataContext = this;

            // Attach to the Opened event, but ensure it only runs once
            this.Opened += async (sender, e) => await InitializeOnce();
        }

        // This method ensures that initialization logic only runs once
        private async Task InitializeOnce()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                await HandleFirstLaunchOrLoadLastProfile();
            }
        }

        private AppSettings LoadAppSettings()
        {
            return AppSettings.Load();  // Load from JSON or other storage
        }

        private async Task HandleFirstLaunchOrLoadLastProfile()
        {
            if (_appSettings.FirstLaunch)
            {
                await ShowMessageBox("Welcome! Let's create your first profile.", "Welcome");

                var profileDialog = new ProfileDialog("Add Profile");
                await profileDialog.ShowDialog(this);

                _appSettings.FirstLaunch = false;
                _appSettings.Save();  // Save the updated settings after first launch
            }
            else if (!string.IsNullOrEmpty(_appSettings.LastUsedProfile))
            {
                var profiles = _profileManager.GetProfilesList();
                if (profiles.Contains(_appSettings.LastUsedProfile))
                {
                    var lastProfile = _profileManager.LoadProfile(_appSettings.LastUsedProfile);
                    LoadProfile(lastProfile);
                    await ShowMessageBox($"Profile '{_appSettings.LastUsedProfile}' loaded.", "Info");
                }
                else
                {
                    await ShowMessageBox("The last used profile no longer exists. Please create or load a new profile.", "Profile Not Found");

                    var profileDialog = new ProfileDialog("Add Profile");
                    await profileDialog.ShowDialog(this);
                }
            }
        }

        private void LoadProfile(Profile profile)
        {
            // Set profile properties in the UI
            IncomingUrlTextBox.Text = profile.IncomingUrl;
            GeneratePTSCheckBox.IsChecked = profile.GeneratePTS;

            OutputGroups.Clear();

            // Set the loaded profile as the current profile
            _appSettings.LastUsedProfile = profile.ProfileName;
            _appSettings.Save();

            foreach (var group in profile.OutputGroups)
            {
                OutputGroups.Add(group);
            }
        }

        private async void ShowProfileDialog(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string? action = button.Content?.ToString();

            if (string.IsNullOrEmpty(action))
            {
                await ShowMessageBox("An unknown action was triggered.", "Error");
                return;
            }

            var dialog = new ProfileDialog(action);
            await dialog.ShowDialog(this);
        }

        private void AddOutputGroup_Click(object sender, RoutedEventArgs e)
        {
            var newGroup = new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" };
            OutputGroups.Add(newGroup);
        }

        private async Task ShowMessageBox(string message, string title)
        {
            await MessageBox.ShowDialog(this, message, MessageBoxButtons.OK);
        }
    }
}
