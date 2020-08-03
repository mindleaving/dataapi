using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SharedWpfControls.Converters
{
    public class BooleanToRedGreenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Brushes.Black;
            return boolValue ? Brushes.Green : Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
