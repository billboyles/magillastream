using System.Reactive;
using ReactiveUI;
using Avalonia.Controls;
using MagillaStream.Views;
using MagillaStream.Utilities;

namespace MagillaStream.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Window _mainWindow;

        // Property stubs for bindings
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

        // Command stubs
        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }

        // Profile-related commands
        public ReactiveCommand<Unit, Unit> AddProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; }

        public MainWindowViewModel(Window mainWindow)
        {
            _mainWindow = mainWindow;

            // Stub for AddOutputGroupCommand
            AddOutputGroupCommand = ReactiveCommand.Create(() =>
            {
                // Placeholder for adding output group logic
            });

            // Profile button commands with the corresponding title
            AddProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Add Profile"));
            LoadProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Load Profile"));
            SaveProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Save Profile"));
            DeleteProfileCommand = ReactiveCommand.Create(() => OpenProfileDialog("Delete Profile"));
        }

        // This method will open the profile dialog with the appropriate title
        private void OpenProfileDialog(string title)
        {
            var profileViewModel = new ProfileViewModel(title);  
            var profileDialog = new ProfileDialog(profileViewModel);  
            profileDialog.Title = title;  
            profileDialog.ShowDialog(_mainWindow);  
        }
    }
}
