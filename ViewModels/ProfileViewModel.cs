using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using MagillaStream.Models;

namespace MagillaStream.ViewModels
{
    public class ProfileViewModel : ReactiveObject
    {
        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set => this.RaiseAndSetIfChanged(ref _profileName, value);
        }

        public ObservableCollection<OutputGroup> OutputGroups { get; set; } = new ObservableCollection<OutputGroup>();

        public ReactiveCommand<Unit, Unit> AddOutputGroupCommand { get; }

        public ProfileViewModel()
        {
            AddOutputGroupCommand = ReactiveCommand.Create(AddOutputGroup);
        }

        private void AddOutputGroup()
        {
            OutputGroups.Add(new OutputGroup { Name = $"Group {OutputGroups.Count + 1}" });
        }
    }
}
