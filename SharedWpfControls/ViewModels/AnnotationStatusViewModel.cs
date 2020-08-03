using System.Windows.Media;
using SharedViewModels.ViewModels;

namespace SharedWpfControls.ViewModels
{
    public class AnnotationStatusViewModel : NotifyPropertyChangedBase
    {
        private readonly Brush errorBackgroundBrush = Brushes.PaleVioletRed;
        private readonly Brush okBackgroundBrush = Brushes.LimeGreen;
        private readonly Brush errorTextBrush = Brushes.White;
        private readonly Brush okTextBrush = Brushes.White;

        public AnnotationStatusViewModel()
        {
            BackgroundBrush = okBackgroundBrush;
            TextBrush = okTextBrush;
            StatusText = "";
        }

        private Brush backgroundBrush;
        public Brush BackgroundBrush
        {
            get => backgroundBrush;
            private set
            {
                backgroundBrush = value;
                OnPropertyChanged();
            }
        }

        private Brush textBrush;
        public Brush TextBrush
        {
            get => textBrush;
            private set
            {
                textBrush = value;
                OnPropertyChanged();
            }
        }

        private string statusText;
        public string StatusText
        {
            get => statusText;
            private set
            {
                statusText = value;
                OnPropertyChanged();
            }
        }

        public void SetStatus(bool isOk, string errorText)
        {
            if (isOk)
                SetOk();
            else
                SetError(errorText);
        }

        private void SetError(string errorText)
        {
            StatusText = errorText;
            BackgroundBrush = errorBackgroundBrush;
            TextBrush = errorTextBrush;
        }

        private void SetOk()
        {
            StatusText = "Looks good!";
            BackgroundBrush = okBackgroundBrush;
            TextBrush = okTextBrush;
        }
    }
}
