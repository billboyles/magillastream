using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Backend;

namespace GUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _obsStreamUrl;
        private string _selectedService;
        private string _streamKey;
        private string _bitrate;
        private string _selectedResolution;
        private bool _reEncode;
        private string _selectedEncoder;
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

        public string StreamKey
        {
            get => _streamKey;
            set
            {
                _streamKey = value;
                OnPropertyChanged(nameof(StreamKey));
            }
        }

        public string Bitrate
        {
            get => _bitrate;
            set
            {
                _bitrate = value;
                OnPropertyChanged(nameof(Bitrate));
            }
        }

        public bool ReEncode
        {
            get => _reEncode;
            set
            {
                _reEncode = value;
                OnPropertyChanged(nameof(ReEncode));
            }
        }

        public ObservableCollection<string> OutputServices { get; set; }
        public ObservableCollection<string> Encoders { get; set; }
        public ObservableCollection<string> Resolutions { get; set; }

        public string SelectedService
        {
            get => _selectedService;
            set
            {
                _selectedService = value;
                OnPropertyChanged(nameof(SelectedService));
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

        public string SelectedEncoder
        {
            get => _selectedEncoder;
            set
            {
                _selectedEncoder = value;
                OnPropertyChanged(nameof(SelectedEncoder));
            }
        }

        public ICommand StartStreamCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

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
                    StreamKey,          // Stream key entered by the user
                    ReEncode,           // Re-encode flag
                    Bitrate,            // Bitrate
                    SelectedResolution, // Resolution
                    SelectedEncoder     // Encoder
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
