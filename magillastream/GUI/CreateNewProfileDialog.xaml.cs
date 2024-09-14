using System.Windows;
using Backend.Utilities;

namespace GUI
{
    public partial class CreateNewProfileDialog : Window
    {
        public string ProfileName { get; private set; }
        public string UserPassword { get; private set; }

        public CreateNewProfileDialog()
        {
            InitializeComponent();
            Logger.LogInfo("CreateNewProfileDialog initialized.");
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == ConfirmPasswordBox.Password)
            {
                if (string.IsNullOrWhiteSpace(ProfileNameBox.Text))
                {
                    MessageBox.Show("Profile name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ProfileName = ProfileNameBox.Text;
                UserPassword = PasswordBox.Password;

                // Create the profile
                var profileManager = new ProfileManager();
                profileManager.CreateProfile(ProfileName, UserPassword); // This ensures the profile and settings.enc are created

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("Profile creation was canceled.");
            DialogResult = false;
        }
    }
}
