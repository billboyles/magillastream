using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using MagillaStream.Models;
using MagillaStream.Utilities;
using System.Linq;

namespace MagillaStream.ViewModels
{
    public class ProfileViewModel : ReactiveObject
    {
        // Properties for TextBox and ComboBox visibility
        private bool _showTextBox;
        public bool ShowTextBox
        {
            get => _showTextBox;
            set
            {
                Logger.Debug($"Setting ShowTextBox to: {value}");
                this.RaiseAndSetIfChanged(ref _showTextBox, value);
            }
        }

        private bool _showComboBox;
        public bool ShowComboBox
        {
            get => _showComboBox;
            set
            {
                Logger.Debug($"Setting ShowComboBox to: {value}");
                this.RaiseAndSetIfChanged(ref _showComboBox, value);
            }
        }

        // The profile name entered by the user or selected from the ComboBox
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get => _profileName;
            set
            {
                Logger.Debug($"Setting ProfileName to: {value}");
                this.RaiseAndSetIfChanged(ref _profileName, value);
            }
        }

        public string IncomingURL { get; set; }
        public bool GeneratePTS { get; set; }
        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();

        public ObservableCollection<string> AvailableProfiles { get; set; } = new ObservableCollection<string>();
        private readonly ProfileManager _profileManager;

        // Command for the OK button
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        // Command for the Cancel button
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        // Delegate to close the dialog from the ViewModel
        public Action<Profile> CloseDialog { get; set; }

        public string DialogContext { get; }

        // Confirmation message visibility and text
        private bool _showConfirmationMessage;
        public bool ShowConfirmationMessage
        {
            get => _showConfirmationMessage;
            set
            {
                Logger.Debug($"Setting ShowConfirmationMessage to: {value}");
                this.RaiseAndSetIfChanged(ref _showConfirmationMessage, value);
            }
        }

        private string _confirmationMessage;
        public string ConfirmationMessage
        {
            get => _confirmationMessage;
            set
            {
                Logger.Debug($"Setting ConfirmationMessage to: {value}");
                this.RaiseAndSetIfChanged(ref _confirmationMessage, value);
            }
        }

        // Constructor that accepts the dialog context to determine the action
        public ProfileViewModel(string dialogContext, string profileName = "")
        {
            _profileManager = new ProfileManager();
            DialogContext = dialogContext;
            Logger.Info($"ProfileViewModel initialized with context: {dialogContext}");

            // Initialize profile name if provided
            if (!string.IsNullOrEmpty(profileName))
            {
                ProfileName = profileName;
            }

            // Set all visibility properties to false initially
            ShowComboBox = false;
            ShowTextBox = false;
            ShowConfirmationMessage = false;

            // Set visibility for the TextBox and ComboBox based on context
            if (dialogContext == "Load Profile" || dialogContext == "Delete Profile")
            {
                ShowComboBox = true;
                LoadAvailableProfiles();  // Load available profiles for the ComboBox
            }
            else if (dialogContext == "Save Profile")
            {
                ShowConfirmationMessage = true;
                ConfirmationMessage = $"Do you want to overwrite the profile '{ProfileName}'?";
            }
            else if (dialogContext == "Create Profile")
            {
                ShowTextBox = true;
            }

            // Initialize the OK command
            OkCommand = ReactiveCommand.Create(ExecuteOkCommand);

            // Initialize the Cancel command to close the dialog without action
            CancelCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("Cancel button clicked, closing dialog without saving.");
                CloseDialog?.Invoke(null); // Close without doing anything
            });
        }

        // This method executes the correct action based on the dialog context and closes the dialog
        private void ExecuteOkCommand()
        {
            Profile profile = null;

            switch (DialogContext)
            {
                case "Create Profile":
                    Logger.Debug("Creating profile.");
                    profile = CreateProfile();
                    break;
                case "Save Profile":
                    Logger.Debug("Saving profile.");
                    profile = SaveProfile();
                    break;
                case "Load Profile":
                    Logger.Debug("Loading profile.");
                    profile = LoadProfile();
                    break;
                case "Delete Profile":
                    Logger.Debug("Deleting profile.");
                    DeleteProfile();
                    break;
                default:
                    Logger.Error($"Unknown dialog context: {DialogContext}");
                    break;
            }

            // After performing the action, close the dialog and pass back the profile
            CloseDialog?.Invoke(profile);
        }

        // Profile creation logic
        private Profile CreateProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                Logger.Error("Profile name is empty or invalid during profile creation.");
                return null;
            }

            var profile = new Profile
            {
                ProfileName = ProfileName,
            };

            Logger.Debug($"Creating profile with name: {ProfileName}");
            _profileManager.CreateProfile(profile);

            // Update AppSettings after profile creation
            AppSettings.Instance.LastUsedProfile = ProfileName;
            AppSettings.Instance.Save();

            return profile;
        }

        // Profile saving logic
        private Profile SaveProfile()
        {
            // Set ProfileName to LastUsedProfile if it's empty
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                ProfileName = AppSettings.Instance.LastUsedProfile;
                Logger.Debug($"ProfileName was empty, setting it to LastUsedProfile: {ProfileName}");
            }

            // If ProfileName is still empty (in case LastUsedProfile was empty), handle it
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                Logger.Error("Profile name is empty or invalid during profile saving.");
                return null;
            }

            var existingProfile = _profileManager.LoadProfile(ProfileName);
            if (existingProfile != null && !ShowConfirmationMessage)
            {
                ShowConfirmationMessage = true;
                ConfirmationMessage = $"Do you want to overwrite the profile '{ProfileName}'?";
                Logger.Debug($"Confirmation message shown for overwriting profile '{ProfileName}'.");
                return null;  // Wait for confirmation before proceeding
            }

            var profile = new Profile
            {
                ProfileName = ProfileName,
                IncomingUrl = this.IncomingURL,
                GeneratePTS = this.GeneratePTS,
                OutputGroups = OutputGroups.ToList()
            };

            Logger.Debug($"Saving profile with name: {ProfileName}, IncomingURL: {IncomingURL}, GeneratePTS: {GeneratePTS}");
            _profileManager.SaveProfile(profile);

            // Update and save AppSettings
            AppSettings.Instance.LastUsedProfile = ProfileName;
            AppSettings.Instance.Save();

            // Reset confirmation message
            ShowConfirmationMessage = false;
            return profile;
        }

        // Profile loading logic
        private Profile LoadProfile()
        {
            Logger.Debug($"Attempting to load profile: {ProfileName}");
            var profile = _profileManager.LoadProfile(ProfileName);
            if (profile == null)
            {
                Logger.Error($"Failed to load profile: {ProfileName}");
            }
            else
            {
                Logger.Debug($"Profile {ProfileName} loaded successfully.");
            }

            // Update AppSettings after loading
            AppSettings.Instance.LastUsedProfile = ProfileName;
            AppSettings.Instance.Save();

            return profile;
        }

        // Profile deletion logic
        private void DeleteProfile()
        {
            Logger.Debug($"Attempting to delete profile: {ProfileName}");
            _profileManager.DeleteProfile(ProfileName);

            // Update AppSettings after deletion
            AppSettings.Instance.LastUsedProfile = string.Empty;
            AppSettings.Instance.Save();
        }

        // Load available profiles for the ComboBox
        private void LoadAvailableProfiles()
        {
            Logger.Debug("Loading available profiles.");
            AvailableProfiles.Clear();
            var profiles = _profileManager.GetProfilesList();
            foreach (var profile in profiles)
            {
                Logger.Debug($"Available profile: {profile}");
                AvailableProfiles.Add(profile);
            }
            Logger.Info("Available profiles loaded successfully.");
        }
    }
}
