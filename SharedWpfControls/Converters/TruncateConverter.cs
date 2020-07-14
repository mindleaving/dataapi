using System;
using System.Globalization;
using System.Windows.Data;
using SharedViewModels.Extensions;

namespace SharedWpfControls.Converters
{
    public class TruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(parameter is int length))
                return value;
            return value.ToString().Truncate(length);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
