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
                Logger.LogInfo($"Selected profile: {SelectedProfile}");
                DialogResult = true;
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
