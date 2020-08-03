using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using SharedColor = SharedViewModels.Objects.Color;
using MediaColor = System.Windows.Media.Color;

namespace SharedWpfControls.Converters
{
    public class ColorToSolidBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
                return null;
            if (!(value is SharedColor color)) 
                return value;
            return new SolidColorBrush(MediaColor.FromArgb(color.A, color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
