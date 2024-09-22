using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MagillaStream.Models;
using MagillaStream.ViewModels;
using MagillaStream.Utilities;  

namespace MagillaStream.Views
{
    public partial class App : Application
    {
        private IClassicDesktopStyleApplicationLifetime _desktop;  // Class-level field

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _desktop = desktop;  // Store reference to desktop
                _desktop.MainWindow = new MainWindow();
                _desktop.Exit += OnExit;  // Hook into the Exit event
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            // No need to check CurrentProfileName; directly use AppSettings.LastUsedProfile
            var appSettings = AppSettings.Instance;
            
            // Save settings when exiting
            appSettings.Save();  // Save the settings
            Logger.Debug($"AppSettings saved with the following values: LastUsedProfile: {appSettings.LastUsedProfile}; FirstLaunch: {appSettings.FirstLaunch}");
        }
    }
}
