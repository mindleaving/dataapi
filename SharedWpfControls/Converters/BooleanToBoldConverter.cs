using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharedWpfControls.Converters
{
    public class BooleanToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is bool boolean))
                return value;
            return boolean == true ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
