using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MagillaStream.ViewModels;

namespace MagillaStream.Views
{
    public partial class ProfileDialog : Window
    {
        public ProfileDialog(ProfileViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Set the CloseDialog action to close the window from the ViewModel
            if (viewModel != null)
            {
                viewModel.CloseDialog = this.Close;
            }
        }

        // Handle the Cancel button click to close the dialog without any action
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the dialog
        }
    }
}
