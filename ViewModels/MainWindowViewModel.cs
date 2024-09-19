using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using MagillaStream.Models;

namespace MagillaStream.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private bool _generatePTS;
        public bool GeneratePTS
        {
            get => _generatePTS;
            set => this.RaiseAndSetIfChanged(ref _generatePTS, value);
        }

        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();

        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }

        public MainWindowViewModel()
        {
            AddOutputGroupCommand = ReactiveCommand.Create(AddOutputGroup);
        }

        private void AddOutputGroup()
        {
            OutputGroups.Add(new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" });
        }
    }
}
