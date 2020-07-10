using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DataProcessingServiceMonitor.ViewModels;

namespace DataProcessingServiceMonitor.Converters
{
    public class LogEntryTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is LogEntryViewModel logEntry))
                return value;
            return logEntry.IsError ? Brushes.Red : Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
