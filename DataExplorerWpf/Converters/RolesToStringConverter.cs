using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using DataAPI.DataStructures.UserManagement;

namespace DataExplorerWpf.Converters
{
    public class RolesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is IEnumerable<Role> roles))
                return value.ToString();
            return string.Join(", ", roles);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
