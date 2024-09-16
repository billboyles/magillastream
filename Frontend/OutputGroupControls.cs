using System.Collections.Generic;
using System.Windows.Controls;

namespace Frontend
{
    public class OutputGroupControls
    {
        // Properties to hold control references
        public GroupBox GroupBox { get; set; }
        public CheckBox ForwardOriginalCheckBox { get; set; }
        public StackPanel EncodingSettingsPanel { get; set; }
        public ComboBox VideoEncoderComboBox { get; set; }
        public TextBox ResolutionTextBox { get; set; }
        public TextBox FpsTextBox { get; set; }
        public TextBox VideoBitrateTextBox { get; set; }
        public ComboBox AudioCodecComboBox { get; set; }
        public TextBox AudioBitrateTextBox { get; set; }
        public List<OutputUrlControls> OutputUrlControlsList { get; set; } = new List<OutputUrlControls>();
    }
}
