using System.Reactive;
using System.Collections.ObjectModel;
using ReactiveUI;
using Avalonia.Controls;
using MagillaStream.Views;
using MagillaStream.Models;
using MagillaStream.Utilities;

namespace MagillaStream.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Window _mainWindow;
        private readonly ProfileManager _profileManager;

        // Property to track the currently loaded profile
        private string _currentProfileName = string.Empty;
        public string CurrentProfileName
        {
            get => _currentProfileName;
            set
            {
                Logger.Debug($"Setting CurrentProfileName to: {value}");
                this.RaiseAndSetIfChanged(ref _currentProfileName, value);
                AppSettings.Instance.LastUsedProfile = value;  // Directly update AppSettings when the profile changes
            }
        }

        private string _incomingURL = string.Empty;
        public string IncomingURL
        {
            get => _incomingURL;
            set
            {
                Logger.Debug($"Setting IncomingURL to: {value}");
                this.RaiseAndSetIfChanged(ref _incomingURL, value);
            }
        }

        private bool _generatePTS;
        public bool GeneratePTS
        {
            get => _generatePTS;
            set
            {
                Logger.Debug($"Setting GeneratePTS to: {value}");
                this.RaiseAndSetIfChanged(ref _generatePTS, value);
            }
        }

        // Property to track Output Groups
        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();

        public ReactiveCommand<Unit, Unit> CreateProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _profileManager = new ProfileManager();

            Logger.Info($"AppSettings loaded with the following values: LastUsedProfile: {AppSettings.Instance.LastUsedProfile}; FirstLaunch: {AppSettings.Instance.FirstLaunch}");

            CreateProfileCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("CreateProfileCommand triggered");
                OpenProfileDialog("Create Profile");
            });

            LoadProfileCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("LoadProfileCommand triggered");
                OpenProfileDialog("Load Profile");
            });

            SaveProfileCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("SaveProfileCommand triggered");
                OpenProfileDialog("Save Profile");
            });

            DeleteProfileCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("DeleteProfileCommand triggered");
                OpenProfileDialog("Delete Profile");
            });

            AddOutputGroupCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("AddOutputGroupCommand triggered");
                OutputGroups.Add(new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" });
            });

            LoadLastUsedProfile();
        }

        // Open the profile dialog based on the dialog context
        private async void OpenProfileDialog(string dialogContext)
        {
            var profileViewModel = new ProfileViewModel(dialogContext)
            {
                IncomingURL = this.IncomingURL,
                GeneratePTS = this.GeneratePTS,
                OutputGroups = new ObservableCollection<OutputGroup>(this.OutputGroups)
            };

            profileViewModel.CloseDialog = (profile) =>
            {
                if (profile != null)
                {
                    Logger.Debug($"Applying profile: {profile.ProfileName}");
                    ApplyProfileToGui(profile);  // Apply the new profile settings to the GUI

                    // Update and save AppSettings after applying the profile
                    AppSettings.Instance.LastUsedProfile = profile.ProfileName;
                }
                else
                {
                    Logger.Error("Received null profile from dialog, no changes applied.");
                }
            };

            var profileDialog = new ProfileDialog(profileViewModel);
            await profileDialog.ShowDialog(_mainWindow);  // Await the completion of the dialog

            // Save settings after the dialog has completed and profile is applied
            AppSettings.Instance.Save();
        }

        // Apply the loaded profile data to the GUI
        private void ApplyProfileToGui(Profile profile)
        {
            IncomingURL = profile.IncomingUrl;
            GeneratePTS = profile.GeneratePTS;
            CurrentProfileName = profile.ProfileName;

            OutputGroups.Clear();
            foreach (var group in profile.OutputGroups)
            {
                OutputGroups.Add(group);
            }

            Logger.Debug($"Profile {profile.ProfileName} applied to the GUI.");
        }

        // Load the last used profile (if it exists)
        private void LoadLastUsedProfile()
        {
            Logger.Debug("Attempting to load last used profile.");
            string lastProfile = AppSettings.Instance.LastUsedProfile;
            Logger.Debug($"Last used profile from AppSettings: {lastProfile}");

            if (!string.IsNullOrWhiteSpace(lastProfile))
            {
                Logger.Debug($"Loading profile: {lastProfile}");
                LoadProfile(lastProfile);
            }
            else
            {
                Logger.Debug("No last used profile found.");
            }
        }

        // Load the profile by name
        private void LoadProfile(string profileName)
        {
            Logger.Debug($"Attempting to load profile: {profileName}");
            var profile = _profileManager.LoadProfile(profileName);
            if (profile != null)
            {
                Logger.Debug($"Profile {profileName} loaded successfully.");
                ApplyProfileToGui(profile);
                AppSettings.Instance.LastUsedProfile = profileName; // Directly update AppSettings when the profile is loaded
                AppSettings.Instance.Save();  // Save the updated settings
            }
            else
            {
                Logger.Error($"Failed to load profile: {profileName}");
            }
        }
    }
}
