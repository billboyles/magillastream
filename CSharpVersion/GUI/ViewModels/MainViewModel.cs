using Backend;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _obsStreamUrl;
        private string _selectedService;
        private string _selectedEncoder;
        private string _selectedResolution;
        private FFmpegService _ffmpegService;

        public string ObsStreamUrl
        {
            get => _obsStreamUrl;
            set
            {
                _obsStreamUrl = value;
                OnPropertyChanged(nameof(ObsStreamUrl));
            }
        }

        public ObservableCollection<string> OutputServices { get; set; }
        public ObservableCollection<string> Encoders { get; set; }
        public ObservableCollection<string> Resolutions { get; set; }  // Added Resolutions collection

        public string SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged(nameof(SelectedService));
            }
        }

        public string SelectedEncoder
        {
            get => _selectedEncoder;
            set
            {
                _selectedEncoder = value;
                OnPropertyChanged(nameof(SelectedEncoder));
            }
        }

        public string SelectedResolution
        {
            get => _selectedResolution;
            set
            {
                _selectedResolution = value;
                OnPropertyChanged(nameof(SelectedResolution));
            }
        }

        public ICommand StartStreamCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _ffmpegService = new FFmpegService();

            OutputServices = new ObservableCollection<string> { "YouTube", "Twitch" };
            Encoders = new ObservableCollection<string>(_ffmpegService.GetAvailableEncoders());

            // Populate the Resolutions collection with standard video resolutions
            Resolutions = new ObservableCollection<string> 
            {
                "360p",
                "480p",
                "720p",
                "1080p",
                "1440p",
                "4K"
            };

            StartStreamCommand = new RelayCommand(StartStreaming);
        }

        private void StartStreaming(object parameter)
        {
            var outputServices = new List<Tuple<string, string, bool, string, string, string>>
            {
                
                new Tuple<string, string, bool, string, string, string>(
                    SelectedService,    // Service URL or identifier
                    "YourStreamKey",    // Replace with actual stream key
                    true,               // Assume re-encode is true for this example
                    "6000k",            // Bitrate (you could use the user input instead)
                    SelectedResolution,  // Resolution from the dropdown
                    SelectedEncoder      // Encoder from the dropdown
                )
            };

            _ffmpegService.StartStream(ObsStreamUrl, outputServices, enablePTSGeneration: true);
        }


        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
