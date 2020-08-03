using System;
using System.Globalization;
using System.Windows.Data;
using Commons.Physics;

namespace SharedWpfControls.Converters
{
    public class UnitPresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is IUnitDefinition unit))
                return value;
            return unit.StringRepresentation;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}