using System;
using System.Globalization;

namespace SharedViewModels.Helpers
{
    public static class ValueParsers
    {
        public static double ParseDouble(string value, double defaultValue)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue)
                ? doubleValue
                : defaultValue;
        }

        public static DateTime ParseDateTime(string value, DateTime defaultValue)
        {
            return DateTime.TryParse(value, out var dateTimeValue)
                ? dateTimeValue
                : defaultValue;
        }

        public static int ParseInt(string value, int defaultValue)
        {
            return int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue)
                ? intValue
                : defaultValue;
        }
    }
}
