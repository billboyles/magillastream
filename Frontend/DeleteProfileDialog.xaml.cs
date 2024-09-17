using System.Windows;
using Utilities;

namespace Frontend
{
    public partial class DeleteProfileDialog : Window
    {
        private ProfileManager _profileManager; 

        public string SelectedProfile { get; private set; }

        public DeleteProfileDialog()
        {
            InitializeComponent();
            _profileManager = new ProfileManager(); 

            // Populate ComboBox with profiles
            ProfilesComboBox.ItemsSource = _profileManager.GetProfilesList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedProfile = ProfilesComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(SelectedProfile))
            {
                MessageBox.Show("Please select a profile to delete.");
                return;
            }

            // Confirm deletion
            var result = MessageBox.Show($"Are you sure you want to delete the profile '{SelectedProfile}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _profileManager.DeleteProfile(SelectedProfile);
                    MessageBox.Show($"Profile '{SelectedProfile}' deleted successfully.");
                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting profile: {ex.Message}");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Close dialog without deleting
        }
    }
}
