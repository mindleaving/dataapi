using System;
using System.Globalization;
using System.Windows.Data;
using Commons.Extensions;
using Commons.Physics;

namespace SharedWpfControls.Converters
{
    public class SIPrefixPresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is SIPrefix siPrefix))
                return value;
            return siPrefix.StringRepresentation();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
