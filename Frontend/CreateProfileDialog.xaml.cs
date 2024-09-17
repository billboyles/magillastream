using System.Windows;

namespace Frontend
{
    public partial class CreateProfileDialog : Window
    {
        public string ProfileName { get; private set; }

        public CreateProfileDialog()
        {
            InitializeComponent();
        }

        private void CreateProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileName = profileNameTextBox.Text;

            // Validate profile name input
            if (string.IsNullOrEmpty(ProfileName))
            {
                MessageBox.Show("Profile name cannot be empty.");
                return;
            }

            // Close dialog and return success
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog and cancel the operation
            DialogResult = false;
        }
    }
}
