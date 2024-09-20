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
            set => this.RaiseAndSetIfChanged(ref _showTextBox, value);
        }

        private bool _showComboBox;
        public bool ShowComboBox
        {
            get => _showComboBox;
            set => this.RaiseAndSetIfChanged(ref _showComboBox, value);
        }

        // The profile name entered by the user or selected from the ComboBox
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get => _profileName;
            set => this.RaiseAndSetIfChanged(ref _profileName, value);
        }

        public string IncomingURL { get; set; }
        public bool GeneratePTS { get; set; }
        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();

        public ObservableCollection<string> AvailableProfiles { get; set; } = new ObservableCollection<string>();
        private readonly ProfileManager _profileManager;

        // Command for the OK button
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        // Delegate to close the dialog from the ViewModel
        public Action<Profile> CloseDialog { get; set; }

        public string DialogContext { get; }

        // Constructor that accepts the dialog context to determine the action
        public ProfileViewModel(string dialogContext)
        {
            _profileManager = new ProfileManager();
            DialogContext = dialogContext;

            // Set visibility for the TextBox and ComboBox based on context
            if (dialogContext == "Load Profile" || dialogContext == "Delete Profile")
            {
                ShowComboBox = true;
                ShowTextBox = false;
                LoadAvailableProfiles();  // Load available profiles for the ComboBox
            }
            else
            {
                ShowComboBox = false;
                ShowTextBox = true;
            }

            // Initialize the OK command
            OkCommand = ReactiveCommand.Create(ExecuteOkCommand);
        }

        // This method executes the correct action based on the dialog context and closes the dialog
        private void ExecuteOkCommand()
        {
            Profile profile = null;

            switch (DialogContext)
            {
                case "Create Profile":
                    profile = CreateProfile();
                    break;
                case "Save Profile":
                    profile = SaveProfile();
                    break;
                case "Load Profile":
                    profile = LoadProfile();
                    break;
                case "Delete Profile":
                    DeleteProfile();
                    break;
            }

            // After performing the action, close the dialog and pass back the profile
            CloseDialog?.Invoke(profile);
        }

        // Methods for each profile operation
        private Profile CreateProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                // Handle case where profile name is empty or invalid
                return null;
            }

            var profile = new Profile
            {
                ProfileName = ProfileName,
                // Add any other profile-specific data here (e.g., OutputGroups)
            };

            _profileManager.CreateProfile(profile);
            return profile;
        }

        private Profile SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(ProfileName))
            {
                return null; // Handle error for invalid profile name
            }

            var profile = new Profile
            {
                ProfileName = ProfileName,
                IncomingUrl = this.IncomingURL,  // Save the state passed from MainWindow
                GeneratePTS = this.GeneratePTS,
                OutputGroups = OutputGroups.ToList()
            };

            _profileManager.SaveProfile(profile);

            return profile;
        }

        private Profile LoadProfile()
        {
            var profile = _profileManager.LoadProfile(ProfileName);
            return profile;
        }

        private void DeleteProfile()
        {
            _profileManager.DeleteProfile(ProfileName);
        }

        // Load available profiles for the ComboBox
        private void LoadAvailableProfiles()
        {
            AvailableProfiles.Clear();
            var profiles = _profileManager.GetProfilesList();
            foreach (var profile in profiles)
            {
                AvailableProfiles.Add(profile);
            }
        }
    }
}
