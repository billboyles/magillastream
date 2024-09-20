using System;
using Avalonia.Controls;
using MagillaStream.ViewModels;

namespace MagillaStream.Views
{
    public partial class ProfileDialog : Window
    {
        // Optionally allow passing in the ViewModel for flexibility
        public ProfileDialog(ProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }

    }
}
