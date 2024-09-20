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
        private string _profileName = string.Empty;
        public string ProfileName
        {
            get => _profileName;
            set => this.RaiseAndSetIfChanged(ref _profileName, value);
        }

        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();
        public ObservableCollection<string> AvailableProfiles { get; set; } = new ObservableCollection<string>();

        private readonly ProfileManager _profileManager;

        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadProfileCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteProfileCommand { get; }

        // Visibility properties for UI elements
        private bool _showComboBox;
        public bool ShowComboBox
        {
            get => _showComboBox;
            set => this.RaiseAndSetIfChanged(ref _showComboBox, value);
        }

        private bool _showTextBox;
        public bool ShowTextBox
        {
            get => _showTextBox;
            set => this.RaiseAndSetIfChanged(ref _showTextBox, value);
        }

        // Constructor accepting context (e.g., "Add Profile", "Load Profile")
        public ProfileViewModel(string dialogContext)
        {
            _profileManager = new ProfileManager();
            
            // Determine which UI elements to show based on the dialog context
            if (dialogContext == "Load Profile" || dialogContext == "Delete Profile")
            {
                ShowComboBox = true;
                ShowTextBox = false;
            }
            else
            {
                ShowComboBox = false;
                ShowTextBox = true;
            }

            AddOutputGroupCommand = ReactiveCommand.Create(AddOutputGroup);
            SaveProfileCommand = ReactiveCommand.Create(SaveProfile);
            LoadProfileCommand = ReactiveCommand.Create(LoadProfile);
            DeleteProfileCommand = ReactiveCommand.Create(DeleteProfile);

            LoadAvailableProfiles();
        }

        private void AddOutputGroup()
        {
            OutputGroups.Add(new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" });
        }

        private void SaveProfile()
        {
            var profile = new Profile
            {
                ProfileName = _profileName,
                OutputGroups = OutputGroups.ToList()
            };

            _profileManager.SaveProfile(profile);
            LoadAvailableProfiles(); // Refresh the list after saving
        }

        private void LoadProfile()
        {
            var profile = _profileManager.LoadProfile(_profileName);
            if (profile != null)
            {
                OutputGroups.Clear();
                foreach (var group in profile.OutputGroups)
                {
                    OutputGroups.Add(group);
                }
            }
        }

        private void DeleteProfile()
        {
            _profileManager.DeleteProfile(_profileName);
            LoadAvailableProfiles();
        }

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
