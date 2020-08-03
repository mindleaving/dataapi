using System;
using System.Globalization;
using System.Windows.Data;

namespace SharedWpfControls.Converters
{
    public class DisplayFunctionValueConverter : IValueConverter
    {
        private readonly Func<object, string> displayNameFunc;

        public DisplayFunctionValueConverter(Func<object, string> displayNameFunc)
        {
            this.displayNameFunc = displayNameFunc;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return displayNameFunc(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}