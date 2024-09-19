using Avalonia.Controls;
using Avalonia.Interactivity;
using MagillaStream.Models;
using MagillaStream.Utilities;

namespace MagillaStream.Views
{
    public partial class ProfileDialog : Window
    {
        public string ProfileName { get; private set; } = string.Empty;
        private readonly ProfileManager _profileManager = new ProfileManager();
        private readonly string? _initialProfileName;
        private readonly string? _action;
        private AppSettings? _appSettings;

        public ProfileDialog() // Parameterless constructor
        {
            InitializeComponent();
        }

        public ProfileDialog(string action)
        {
            InitializeComponent();
            _action = action;

            // Load the AppSettings
            _appSettings = AppSettings.Load() ?? new AppSettings();  
            _initialProfileName = _appSettings.LastUsedProfile;  // Load the last used profile from settings

            UpdateDialogForAction(action);
        }

        private void UpdateDialogForAction(string action)
        {
            switch (action)
            {
                case "Add Profile":
                    this.Title = "Add New Profile";
                    ProfileNameTextBox.IsVisible = true;
                    ProfileDropdown.IsVisible = false;
                    ProfileNameTextBox.Focus();  // Focus on the text box for profile name input
                    break;

                case "Load Profile":
                case "Delete Profile":
                    this.Title = action == "Load Profile" ? "Load Profile" : "Delete Profile";
                    ProfileDropdown.IsVisible = true;  // Make the dropdown visible
                    ProfileNameTextBox.IsVisible = false;
                    var profiles = _profileManager.GetProfilesList();
                    ProfileDropdown.ItemsSource = profiles;  // Bind list of profiles to dropdown
                    break;

                case "Save Profile":
                    this.Title = "Save Profile";
                    
                    // Hide ProfileNameTextBox if we are saving an existing profile
                    if (!string.IsNullOrEmpty(_initialProfileName))
                    {
                        ProfileNameTextBox.IsVisible = false;  // Hide text box if we are just confirming a save
                        ProfileDropdown.IsVisible = false;  // Hide dropdown if we are just confirming a save
                        DialogInstructionText.Text = $"Confirm save for profile '{_initialProfileName}'?";
                    }
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle the action depending on the type
            switch (_action)
            {
                case "Add Profile":
                    HandleAddProfile();
                    break;
                case "Load Profile":
                    HandleLoadProfile();
                    break;
                case "Delete Profile":
                    HandleDeleteProfile();
                    break;
                case "Save Profile":
                    HandleSaveProfile();
                    break;
            }
        }

        private async void HandleSaveProfile()
        {
            // Use the last used profile instead of asking for a name again
            if (string.IsNullOrEmpty(_initialProfileName))
            {
                await MessageBox.ShowDialog(this, "No profile is currently loaded.", MessageBoxButtons.OK);
                return;
            }

            // Check if the profile already exists
            if (_profileManager.GetProfilesList().Contains(_initialProfileName))
            {
                // Confirm with the user before overwriting the profile
                var confirmation = await MessageBox.ShowDialog(this, $"Profile '{_initialProfileName}' already exists. Do you want to overwrite it?", MessageBoxButtons.YesCancel);
                if (confirmation == MessageBoxResult.Yes)
                {
                    _profileManager.SaveProfile(new Profile { ProfileName = _initialProfileName });
                    await MessageBox.ShowDialog(this, $"Profile '{_initialProfileName}' saved successfully.", MessageBoxButtons.OK);
                    Close();
                }
            }
            else
            {
                // If profile doesn't exist, ask the user to create one and redirect to AddProfileHandler
                var confirmation = await MessageBox.ShowDialog(this, $"Profile '{_initialProfileName}' does not exist. Do you want to create a new profile?", MessageBoxButtons.YesCancel);
                if (confirmation == MessageBoxResult.Yes)
                {
                    // Run AddProfileHandler to add the profile
                    HandleAddProfile();
                }
            }
        }

        private async void HandleAddProfile()
        {
            ProfileName = ProfileNameTextBox.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                await MessageBox.ShowDialog(this, "Profile name cannot be empty.", MessageBoxButtons.OK);
                return;
            }

            // Check if profile already exists
            if (_profileManager.GetProfilesList().Contains(ProfileName))
            {
                await MessageBox.ShowDialog(this, "Profile already exists!", MessageBoxButtons.OK);
            }
            else
            {
                // Add the profile
                var newProfile = new Profile { ProfileName = ProfileName };
                _profileManager.CreateProfile(newProfile);

                // Set the new profile as the last used profile
                _appSettings.LastUsedProfile = ProfileName;
                _appSettings.Save();

                // Automatically load the newly created profile
                _profileManager.LoadProfile(ProfileName);

                await MessageBox.ShowDialog(this, $"Profile '{ProfileName}' added and loaded successfully.", MessageBoxButtons.OK);
                Close();
            }
        }

        private async void HandleLoadProfile()
        {
            // Load the selected profile from the dropdown
            var selectedProfile = ProfileDropdown.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedProfile))
            {
                await MessageBox.ShowDialog(this, "Please select a profile to load.", MessageBoxButtons.OK);
                return;
            }

            // Set the loaded profile as the current profile
            ProfileName = selectedProfile;
            _profileManager.LoadProfile(ProfileName);

            // Update LastUsedProfile and save it to AppSettings
            _appSettings.LastUsedProfile = ProfileName;
            _appSettings.Save();

            await MessageBox.ShowDialog(this, $"Profile '{ProfileName}' loaded successfully.", MessageBoxButtons.OK);
            Close();
        }

        private async void HandleDeleteProfile()
        {
            // Delete the selected profile
            var selectedProfile = ProfileDropdown.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedProfile))
            {
                await MessageBox.ShowDialog(this, "Please select a profile to delete.", MessageBoxButtons.OK);
                return;
            }

            var confirmation = await MessageBox.ShowDialog(this, $"Are you sure you want to delete '{selectedProfile}'?", MessageBoxButtons.YesCancel);
            if (confirmation == MessageBoxResult.Yes)
            {
                _profileManager.DeleteProfile(selectedProfile);
                await MessageBox.ShowDialog(this, $"Profile '{selectedProfile}' deleted successfully.", MessageBoxButtons.OK);
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileName = string.Empty;
            Close();
        }
    }
}
