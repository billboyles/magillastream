using Avalonia.Controls;
using MagillaStream.ViewModels;

namespace MagillaStream.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);  // Pass the current window to the ViewModel
        }
    }
}
