using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DataExplorerWpf.Converters
{
    public class PermissionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PermissionType permissionType))
                return Brushes.Black;
            switch (permissionType)
            {
                case PermissionType.None:
                    return Brushes.Red;
                case PermissionType.Read:
                    return Brushes.LimeGreen;
                case PermissionType.Write:
                    return Brushes.LimeGreen;
                case PermissionType.ReadWrite:
                    return Brushes.DarkGreen;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
