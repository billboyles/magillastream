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

        // Directly expose LastUsedProfile from AppSettings for GUI binding
        public string LastUsedProfile
        {
            get => AppSettings.Instance.LastUsedProfile;
            set
            {
                Logger.Debug($"Setting LastUsedProfile to: {value}");
                AppSettings.Instance.LastUsedProfile = value;
                this.RaisePropertyChanged(nameof(LastUsedProfile));
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
                    Logger.Debug($"Profile received from dialog: {profile.ProfileName}");
                    
                    // Apply profile settings to the GUI
                    ApplyProfileToGui(profile);

                    // Update LastUsedProfile and save AppSettings
                    AppSettings.Instance.LastUsedProfile = profile.ProfileName;
                    AppSettings.Instance.Save();
                }
                else
                {
                    Logger.Error("Received null profile from dialog, no changes applied.");
                }
            };

            var profileDialog = new ProfileDialog(profileViewModel);
            await profileDialog.ShowDialog(_mainWindow);
        }

        // Apply the loaded profile data to the GUI
        private void ApplyProfileToGui(Profile profile)
        {
            // Use the properties to ensure RaiseAndSetIfChanged is triggered for UI updates
            IncomingURL = profile.IncomingUrl ?? "No URL Provided";  // Set IncomingURL via property
            GeneratePTS = profile.GeneratePTS;                        // Set GeneratePTS via property

            // Clear and add output groups
            OutputGroups.Clear();
            foreach (var group in profile.OutputGroups)
            {
                OutputGroups.Add(group);  // Add each output group from the profile
            }

            Logger.Debug($"Profile {profile.ProfileName} applied to the GUI: {profile.IncomingUrl}, {profile.GeneratePTS}.");
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
                LastUsedProfile = profileName;  // Directly update LastUsedProfile when the profile is loaded
                AppSettings.Instance.Save();  // Save the updated settings
            }
            else
            {
                Logger.Error($"Failed to load profile: {profileName}");
            }
        }
    }
}
