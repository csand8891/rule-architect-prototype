using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrWhiteSpace(value as string);
            bool hideIfEmpty = parameter as string == "NotEmpty" ? false : true; // Default: hide if empty

            if (hideIfEmpty)
            {
                return isEmpty ? Visibility.Collapsed : Visibility.Visible;
            }
            else // Hide if NOT empty (used for "NotEmpty" parameter)
            {
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}