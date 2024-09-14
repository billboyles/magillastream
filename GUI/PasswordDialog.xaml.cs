using System.Windows;
using Backend.Utilities;

namespace GUI
{
    public partial class PasswordDialog : Window
    {
        public string UserPassword { get; private set; }

        public PasswordDialog()
        {
            InitializeComponent();
            Logger.LogInfo("PasswordDialog initialized.");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserPassword = PasswordBox.Password;
            Logger.LogInfo("Password entered.");
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.LogInfo("PasswordDialog canceled.");
            DialogResult = false;
        }
    }
}
