using System;
using System.Globalization;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters // Or RuleArchitectPrototype.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false; // Or DependencyProperty.UnsetValue, or throw exception
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }
            return false; // Or DependencyProperty.UnsetValue, or throw exception
        }
    }
}