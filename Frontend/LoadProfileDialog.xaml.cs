using System.Collections.Generic;
using System.Windows;

namespace Frontend
{
    public partial class LoadProfileDialog : Window
    {
        public string SelectedProfile { get; private set; }

        public LoadProfileDialog(List<string> profiles)
        {
            InitializeComponent();
            ProfilesComboBox.ItemsSource = profiles;
        }

        private void UseProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesComboBox.SelectedItem != null)
            {
                SelectedProfile = ProfilesComboBox.SelectedItem.ToString();
                DialogResult = true; // This closes the dialog
            }
            else
            {
                MessageBox.Show("Please select a profile.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
