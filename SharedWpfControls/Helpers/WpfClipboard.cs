using System.Windows;
using SharedViewModels.Objects;

namespace SharedWpfControls.Helpers
{
    public class WpfClipboard : IClipboard
    {
        public void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
}
