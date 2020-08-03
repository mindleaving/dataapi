using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SharedViewModels.Objects;

namespace SharedWpfControls.Converters
{
    public class FileHandlingStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is FileHandlingStatus status))
                return Brushes.Transparent;
            switch (status)
            {
                case FileHandlingStatus.Success:
                case FileHandlingStatus.AlreadyExists:
                    return Brushes.DarkSeaGreen;
                case FileHandlingStatus.Error:
                    return Brushes.PaleVioletRed;
                case FileHandlingStatus.UserActionRequired:
                    return Brushes.Gold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
