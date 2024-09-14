
using System.Windows;
using Backend.Utilities;

namespace GUI
{
    public partial class ProfileDialog : Window
    {
        public string SelectedProfile => ProfileDropdown.SelectedItem?.ToString(); // Property to get the selected profile
        private ProfileManager _profileManager = new ProfileManager(); // ProfileManager instance

        public ProfileDialog()
        {
            InitializeComponent();
            LoadProfiles();
        }

        // Load profiles from ProfileManager
        private void LoadProfiles()
        {
            Logger.LogInfo("Loading profiles in ProfileDialog");
            var profiles = _profileManager.GetProfiles();
            ProfileDropdown.ItemsSource = profiles;
        }

        // Use the selected profile
        private void UseProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfileDropdown.SelectedItem != null)
            {
                // Prompt the user for a password
                var passwordDialog = new PasswordDialog();
                if (passwordDialog.ShowDialog() == true)
                {
                    string password = passwordDialog.UserPassword;
                    try
                    {
                        var settingsManager = new SettingsManager(password, SelectedProfile);
                        var settings = settingsManager.LoadSettings(); // Attempt to load the settings with the password
                        
                        Logger.LogInfo($"Profile {SelectedProfile} loaded successfully.");
                        DialogResult = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to load profile {SelectedProfile}: {ex.Message}");
                        MessageBox.Show("Failed to load the profile. Please check your password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Logger.LogWarning("Password entry was canceled.");
                    DialogResult = false;
                }
            }
            else
            {
                Logger.LogWarning("No profile selected.");
                MessageBox.Show("Please select a profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handle 'Cancel' button click
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Profile selection canceled.");
            DialogResult = false;
        }

        // Create a new profile
        private void CreateNewProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var createNewProfileDialog = new CreateNewProfileDialog();
            if (createNewProfileDialog.ShowDialog() == true)
            {
                Logger.LogInfo($"New profile created: {createNewProfileDialog.ProfileName}");
                
                // Reload the profiles into the dropdown
                LoadProfiles();
            }
            else
            {
                Logger.LogWarning("CreateNewProfileDialog was canceled.");
            }
        }
    }
}
