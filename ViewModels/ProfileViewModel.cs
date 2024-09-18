using System.ComponentModel;
using System.Windows.Input;
using MagillaStream.Utilities;
using MagillaStream.Models;

namespace MagillaStream.ViewModels
{
    public class ProfileViewModel : INotifyPropertyChanged
    {
        private readonly ProfileManager _profileManager;
        private Profile _currentProfile;

        public Profile CurrentProfile
        {
            get { return _currentProfile; }
            set 
            {
                _currentProfile = value;
                OnPropertyChanged(nameof(CurrentProfile));
            }
        }

        public ICommand SaveProfileCommand { get; set; }

        public ProfileViewModel(ProfileManager profileManager)
        {
            _profileManager = profileManager;

            // Define SaveProfileCommand to use the ProfileManager service
            SaveProfileCommand = new RelayCommand(SaveProfile);
        }

        private void SaveProfile()
        {
            _profileManager.SaveProfile(CurrentProfile);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
