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
        private string _streamKey;
        private string _bitrate;
        private string _resolution;
        private bool _reEncode;

        private FFmpegService _ffmpegService;

        // Properties for OBS Stream URL, Stream Key, Bitrate, Resolution, and Re-encode option
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

        public string Resolution
        {
            get => _resolution;
            set
            {
                _resolution = value;
                OnPropertyChanged(nameof(Resolution));
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

        // Properties for Output Services and Encoders
        public ObservableCollection<string> OutputServices { get; set; }
        public ObservableCollection<string> Encoders { get; set; }

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

        // ICommand for starting the stream
        public ICommand StartStreamCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Constructor to initialize the services and encoders
        public MainViewModel()
        {
            _ffmpegService = new FFmpegService();

            // Initialize Output Services (e.g., YouTube, Twitch)
            OutputServices = new ObservableCollection<string> { "YouTube", "Twitch" };

            // Populate encoders from the backend service
            Encoders = new ObservableCollection<string>(_ffmpegService.GetAvailableEncoders());

            // Initialize the command for starting the stream
            StartStreamCommand = new RelayCommand(StartStreaming);
        }

        // Method to start streaming
        private void StartStreaming(object parameter)
        {
            // Construct the output services tuple using values from the user input
            var outputServices = new List<Tuple<string, string, bool, string, string, string>>
            {
                new Tuple<string, string, bool, string, string, string>(
                    SelectedService,     // Service (e.g., YouTube, Twitch)
                    StreamKey,           // Stream key entered by the user
                    ReEncode,            // Re-encode option (from checkbox)
                    Bitrate,             // Bitrate entered by the user
                    Resolution,          // Resolution selected by the user
                    SelectedEncoder      // Encoder selected by the user
                )
            };

            // Start the stream with the OBS URL and the output services
            _ffmpegService.StartStream(ObsStreamUrl, outputServices, true);  // Enable PTS generation
        }

        // Property change notification
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
