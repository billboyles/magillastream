using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace MagillaStream.Views
{
    public enum MessageBoxResult
    {
        OK,
        Yes,
        No
    }

    public partial class MessageBox : Window
    {
        public MessageBoxResult Result { get; private set; }

        public MessageBox() // Parameterless constructor
        {
            InitializeComponent();
        }
        public MessageBox(string message, MessageBoxButtons buttons)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            SetupButtons(buttons);
        }

        private void SetupButtons(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.OK:
                    OkButton.IsVisible = true;
                    break;
                case MessageBoxButtons.OKCancel:
                    OkButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    break;
                case MessageBoxButtons.YesCancel:
                    YesButton.IsVisible = true;
                    CancelButton.IsVisible = true;
                    break;
                case MessageBoxButtons.YesNo:
                    YesButton.IsVisible = true;
                    NoButton.IsVisible = true;
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        public static async Task<MessageBoxResult> ShowDialog(Window parent, string message, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            var messageBox = new MessageBox(message, buttons);
            await messageBox.ShowDialog(parent);
            return messageBox.Result;
        }
    }

    public enum MessageBoxButtons
    {
        OKCancel,
        YesCancel,
        OK,
        YesNo
    }
}
