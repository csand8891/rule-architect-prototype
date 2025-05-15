using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters
{
    public class IsEmptyToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; } = Visibility.Visible; // Default: Visible when empty/zero
        public Visibility FalseValue { get; set; } = Visibility.Collapsed; // Default: Collapsed when not empty/non-zero

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = false;
            if (value == null)
            {
                isEmpty = true;
            }
            else if (value is int count)
            {
                isEmpty = count == 0;
            }
            else if (value is string str)
            {
                isEmpty = string.IsNullOrWhiteSpace(str);
            }
            else if (value is ICollection collection)
            {
                isEmpty = collection.Count == 0;
            }
            // Add other types if needed

            return isEmpty ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}