using Avalonia.Controls;
using MagillaStream.ViewModels;

namespace MagillaStream.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set DataContext and pass the current window to the ViewModel
            var mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = mainWindowViewModel;
        }
    }
}
