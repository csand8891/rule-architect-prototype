using System;
using System.Globalization;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters
{
    public class NullToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string stringParameter)
            {
                var parts = stringParameter.Split('/');
                if (parts.Length == 2) // Expected format: "ValueIfNull/ValueIfNotNull"
                {
                    return value == null ? parts[0] : parts[1];
                }
            }
            // Fallback if parameter is not in the expected format
            return value == null ? "Default (Is Null)" : "Default (Is Not Null)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
