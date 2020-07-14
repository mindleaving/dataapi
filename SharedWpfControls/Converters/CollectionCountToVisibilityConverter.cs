using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SharedWpfControls.Converters
{
    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new NullReferenceException();
            if(!(value is int count))
                throw new NotSupportedException("Converter only handles integers");
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
