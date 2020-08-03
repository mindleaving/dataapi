using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SharedViewModels.Objects;

namespace FileUploader.Converters
{
    public class IsErrorVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is FileHandlingStatus fileHandlingStatus))
                return value;
            return fileHandlingStatus == FileHandlingStatus.Error
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
