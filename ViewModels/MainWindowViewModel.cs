using System;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ReactiveCommand<OutputGroup, Unit> RemoveOutputGroupCommand { get; }
        public ReactiveCommand<(OutputGroup, StreamTarget), Unit> RemoveStreamTargetCommand { get; }
        public ReactiveCommand<OutputGroup, Unit> AddStreamTargetCommand { get; }

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _profileManager = new ProfileManager();

            Logger.Info($"AppSettings loaded with the following values: LastUsedProfile: {AppSettings.Instance.LastUsedProfile}; FirstLaunch: {AppSettings.Instance.FirstLaunch}");

            // Handle showing the welcome dialog on window opened
            _mainWindow.Opened += (sender, args) =>
            {
                Logger.Debug($"AppSettings.FirstLaunch is {AppSettings.Instance.FirstLaunch}.");
                if (AppSettings.Instance.FirstLaunch)
                {
                    Logger.Info("First launch detected, showing Welcome Dialog.");
                    ShowWelcomeDialog();  // Show the welcome dialog only after the window is opened

                    // Set FirstLaunch to false and save settings
                    AppSettings.Instance.FirstLaunch = false;
                    AppSettings.Instance.Save();
                    Logger.Info("First launch flag set to false and saved.");
                }
                else
                {
                    Logger.Debug("First launch is false, Welcome Dialog will not be shown.");
                }
            };

            // Profile commands
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

            // Output group and stream target commands
            AddOutputGroupCommand = ReactiveCommand.Create(() =>
            {
                Logger.Debug("AddOutputGroupCommand triggered");
                OutputGroups.Add(new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" });
            });

            RemoveOutputGroupCommand = ReactiveCommand.Create<OutputGroup>(outputGroup =>
            {
                Logger.Debug($"Removing OutputGroup: {outputGroup.Name}");
                OutputGroups.Remove(outputGroup);
            });

            AddStreamTargetCommand = ReactiveCommand.Create<OutputGroup>(outputGroup =>
            {
                Logger.Debug($"Adding StreamTarget to OutputGroup: {outputGroup.Name}");
                outputGroup.StreamTargets.Add(new StreamTarget { Url = "New Target URL", StreamKey = "New Stream Key" });
            });

            // Handle tuple binding for removing stream target
            RemoveStreamTargetCommand = ReactiveCommand.Create<(OutputGroup, StreamTarget)>(tuple =>
            {
                var (outputGroup, streamTarget) = tuple;
                Logger.Debug($"Removing StreamTarget from OutputGroup: {outputGroup.Name}");
                outputGroup.StreamTargets.Remove(streamTarget);
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

            // Subscribe to the ProfileDeleted event to reset the GUI
            profileViewModel.ProfileDeleted += (sender, args) =>
            {
                Logger.Debug("Profile deleted, resetting GUI.");
                AppSettings.Instance.LastUsedProfile = "";
                ResetGui();
            };

            // Subscribe to the ProfileSaved event to update the GUI
            profileViewModel.ProfileApplied += (sender, profile) =>
            {
                if (profile != null)
                {
                    Logger.Debug($"MainWindowViewModel - ProfileApplied event triggered with profile: {profile.ProfileName}");
                    ApplyProfileToGui(profile);  // Apply profile
                    AppSettings.Instance.LastUsedProfile = profile.ProfileName;
                    AppSettings.Instance.Save();
                }
                else
                {
                    Logger.Error("MainWindowViewModel - Profile is null.");
                }
            };

            var profileDialog = new ProfileDialog(profileViewModel);
            await profileDialog.ShowDialog(_mainWindow);
        }

        // Apply the loaded profile data to the GUI
        private void ApplyProfileToGui(Profile profile)
        {
            Logger.Debug($"Applying profile to GUI: {profile.ProfileName}.");

            IncomingURL = profile.IncomingUrl ?? "No URL Provided";
            Logger.Debug($"IncomingURL set to: {IncomingURL}");

            GeneratePTS = profile.GeneratePTS;
            Logger.Debug($"GeneratePTS set to: {GeneratePTS}");

            OutputGroups.Clear();
            Logger.Debug("Cleared OutputGroups.");
            
            foreach (var group in profile.OutputGroups)
            {
                OutputGroups.Add(group);
                Logger.Debug($"Added OutputGroup: {group.Name}");
            }

            // Update the LastUsedProfile, which updates the Current Profile field in the UI
            LastUsedProfile = profile.ProfileName;
            Logger.Debug($"LastUsedProfile set to: {LastUsedProfile}");

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

        private void ResetGui()
        {
            Logger.Debug("Resetting GUI to blank state.");

            IncomingURL = string.Empty;
            GeneratePTS = false;
            OutputGroups.Clear(); // Clear the OutputGroups collection
            LastUsedProfile = ""; // Clear the LastUsedProfile

            Logger.Debug("GUI reset completed.");
        }

        // Show the welcome dialog
        public void ShowWelcomeDialog()
        {
            var welcomeViewModel = new WelcomeViewModel();

            var welcomeDialog = new WelcomeDialog
            {
                DataContext = welcomeViewModel
            };

            // Assign a method to close the dialog
            welcomeViewModel.CloseDialog = () =>
            {
                Logger.Debug("Closing WelcomeDialog.");
                welcomeDialog.Close();  // Close the welcome dialog directly
            };

            // Open the "Create Profile" dialog after closing the WelcomeDialog
            welcomeViewModel.OpenCreateProfile = () =>
            {
                Logger.Debug("Opening Create Profile dialog from WelcomeDialog.");
                OpenProfileDialog("Create Profile");
            };

            // Show the welcome dialog
            welcomeDialog.ShowDialog(_mainWindow);
        }
    }
}
