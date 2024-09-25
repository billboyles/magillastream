using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace MagillaStream.Models
{
    public class OutputGroup
    {
        public string Name { get; set; } = string.Empty;
        public bool ForwardOriginal { get; set; } = false;
        public ObservableCollection<StreamTarget> StreamTargets { get; set; } = new ObservableCollection<StreamTarget>();
        public StreamSettings? StreamSettings { get; set; }

        // Commands with OutputGroup as parameter
        public ReactiveCommand<OutputGroup, Unit> AddStreamTargetCommand { get; set; }
        public ReactiveCommand<OutputGroup, Unit> RemoveOutputGroupCommand { get; set; }
        public ReactiveCommand<StreamTarget, Unit> RemoveStreamTargetCommand { get; set; }

        // Constructor accepting commands with parameters
        public OutputGroup(ReactiveCommand<OutputGroup, Unit> addStreamTargetCommand,
                           ReactiveCommand<OutputGroup, Unit> removeOutputGroupCommand,
                           ReactiveCommand<StreamTarget, Unit> removeStreamTargetCommand)
        {
            AddStreamTargetCommand = addStreamTargetCommand;
            RemoveOutputGroupCommand = removeOutputGroupCommand;
            RemoveStreamTargetCommand = removeStreamTargetCommand;

            // Create a default stream target and attach the remove command
            StreamTargets.Add(new StreamTarget(RemoveStreamTargetCommand));
        }

        // Method to validate that the group has valid settings
        public void Validate()
        {
            if (!ForwardOriginal && StreamSettings == null)
            {
                throw new InvalidOperationException("Either ForwardOriginal must be true, or EncodingSettings must be provided.");
            }
        }
    }
}
