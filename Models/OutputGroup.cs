using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
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

        // New property for available encoders
        public ObservableCollection<string> AvailableVideoEncoders { get; set; } = new ObservableCollection<string>();

        // Commands with OutputGroup as parameter
        [JsonIgnore]
        public ReactiveCommand<OutputGroup, Unit> AddStreamTargetCommand { get; set; }

        [JsonIgnore]
        public ReactiveCommand<OutputGroup, Unit> RemoveOutputGroupCommand { get; set; }

        [JsonIgnore]
        public ReactiveCommand<StreamTarget, Unit> RemoveStreamTargetCommand { get; set; }

        // Consolidated constructor accepting commands and available encoders
        public OutputGroup(ReactiveCommand<OutputGroup, Unit> addStreamTargetCommand,
                           ReactiveCommand<OutputGroup, Unit> removeOutputGroupCommand,
                           ReactiveCommand<StreamTarget, Unit> removeStreamTargetCommand,
                           ObservableCollection<string> availableVideoEncoders)
        {
            AddStreamTargetCommand = addStreamTargetCommand;
            RemoveOutputGroupCommand = removeOutputGroupCommand;
            RemoveStreamTargetCommand = removeStreamTargetCommand;

            // Assign the passed encoders to the property
            AvailableVideoEncoders = availableVideoEncoders;

            // Initialize StreamSettings using available encoders
            StreamSettings = new StreamSettings(AvailableVideoEncoders);

            // Add a default stream target
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
