using System.Windows;

namespace GUI
{
    public partial class PasswordDialog : Window
    {
        public string UserPassword { get; private set; }

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UserPassword = PasswordInput.Password; 
            DialogResult = true; 
            Close();
        }
    }
}
