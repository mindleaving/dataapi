using System;
using System.Globalization;
using System.Windows.Data;
using Commons.Physics;
using SharedViewModels.Extensions;

namespace SharedWpfControls.Converters
{
    public class VolumeUnitValuePresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is UnitValue unitValue))
                return value;
            if (!unitValue.CanConvertTo(Unit.Liter))
                return unitValue;
            return unitValue.PresentVolume();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
