using System;
using System.Globalization;
using System.Windows.Data;
using Commons.Physics;

namespace SharedWpfControls.Converters
{
    public class UnitValuePresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            if (!(value is UnitValue unitValue))
                return value.ToString();
            return unitValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UnitValue)
                return value;
            return UnitValue.Parse((string)value);
        }
    }
}
