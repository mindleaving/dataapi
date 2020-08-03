using System;
using System.Globalization;
using System.Windows.Data;
using DataAPI.DataStructures.UserManagement;

namespace SharedWpfControls.Converters
{
    public class UserProfilePresentationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;
            if (!(value is UserProfile userProfile))
                return value.ToString();
            return $"{userProfile.Username} - {userProfile.FirstName} {userProfile.LastName}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
