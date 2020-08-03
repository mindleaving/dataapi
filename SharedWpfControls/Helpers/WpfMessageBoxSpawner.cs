using System;
using System.Windows;
using SharedViewModels.Helpers;
using MessageBoxResult = SharedViewModels.Helpers.MessageBoxResult;

namespace SharedWpfControls.Helpers
{
    public class WpfMessageBoxSpawner : IMessageBoxSpawner
    {
        public void Show(string message)
        {
            MessageBox.Show(message);
        }

        public MessageBoxResult Show(string message, string title, MessageBoxButtons buttons)
        {
            MessageBoxButton convertedButton;
            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    convertedButton = MessageBoxButton.OK;
                    break;
                case MessageBoxButtons.OkCancel:
                    convertedButton = MessageBoxButton.OKCancel;
                    break;
                case MessageBoxButtons.YesNo:
                    convertedButton = MessageBoxButton.YesNo;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    convertedButton = MessageBoxButton.YesNoCancel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
            }
            var result = MessageBox.Show(message, title, convertedButton);
            switch (result)
            {
                case System.Windows.MessageBoxResult.None:
                    return MessageBoxResult.None;
                case System.Windows.MessageBoxResult.OK:
                    return MessageBoxResult.Ok;
                case System.Windows.MessageBoxResult.Cancel:
                    return MessageBoxResult.Cancel;
                case System.Windows.MessageBoxResult.Yes:
                    return MessageBoxResult.Yes;
                case System.Windows.MessageBoxResult.No:
                    return MessageBoxResult.No;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}