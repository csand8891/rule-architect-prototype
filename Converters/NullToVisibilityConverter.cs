using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters // Or RuleArchitectPrototype.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // This converter is typically used one-way, so ConvertBack is often not needed.
            // If you did need it, you'd have to decide what a Visibility value maps back to.
            throw new NotImplementedException();
        }
    }
}