using System.Reactive;
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
        private AppSettings _appSettings;
        private readonly ProfileManager _profileManager;

        // Property to track the currently loaded profile
        private string _currentProfileName = string.Empty;
        public string CurrentProfileName
        {
            get => _currentProfileName;
            set => this.RaiseAndSetIfChanged(ref _currentProfileName, value);
        }

        private string _incomingURL = string.Empty;
        public string IncomingURL
        {
            get => _incomingURL;
            set => this.RaiseAndSetIfChanged(ref _incomingURL, value);
        }

        private bool _generatePTS;
        public bool GeneratePTS
        {
            get => _generatePTS;
            set => this.RaiseAndSetIfChanged(ref _generatePTS, value);
        }

        public ReactiveCommand<Unit, Unit> CreateProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;
            _profileManager = new ProfileManager();
            _appSettings = AppSettings.Load();

            CreateProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Create Profile"));
            LoadProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Load Profile"));
            SaveProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Save Profile"));
            DeleteProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Delete Profile"));

            AddOutputGroupCommand = ReactiveCommand.Create(() =>
            {
                // Logic for adding an output group
            });

            LoadLastUsedProfile();
        }

        // Open the profile dialog based on the dialog context
        private async void OpenProfileDialog(string dialogContext)
        {
            var profileViewModel = new ProfileViewModel(dialogContext);
            
            // Assign a handler to CloseDialog that applies the profile to the GUI
            profileViewModel.CloseDialog = (profile) =>
            {
                if (profile != null)
                {
                    ApplyProfileToGui(profile);  // Update the GUI with the loaded or created profile
                }
            };

            var profileDialog = new ProfileDialog(profileViewModel);
            
            await profileDialog.ShowDialog(_mainWindow);  // Show the dialog asynchronously
        }
        // This method applies the loaded profile to the GUI
        private void ApplyProfileToGui(Profile profile)
        {
            IncomingURL = profile.IncomingUrl;
            GeneratePTS = profile.GeneratePTS;
            CurrentProfileName = profile.ProfileName;
            // Update other GUI elements based on the profile
        }

        private void LoadLastUsedProfile()
        {
            string lastProfile = _appSettings.LastUsedProfile;
            if (!string.IsNullOrWhiteSpace(lastProfile))
            {
                LoadProfile(lastProfile);
            }
        }

        private void LoadProfile(string profileName)
        {
            var profile = _profileManager.LoadProfile(profileName);
            if (profile != null)
            {
                ApplyProfileToGui(profile);
            }
        }
    }
}
