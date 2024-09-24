using System;
using System.Reactive;
using ReactiveUI;

namespace MagillaStream.ViewModels
{
    public class WelcomeViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        // Delegate to close the dialog and notify that profile creation should be opened
        public Action CloseDialog { get; set; }
        public Action OpenCreateProfile { get; set; }

        public WelcomeViewModel()
        {
            // Initialize the OK command to close the dialog and then open Create Profile
            OkCommand = ReactiveCommand.Create(() =>
            {
                CloseDialog?.Invoke();  // Close the welcome dialog
                OpenCreateProfile?.Invoke();  // Notify to open the Create Profile dialog
            });
        }
    }
}
