﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters // Or RuleArchitectPrototype.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && !b) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is Visibility v && v == Visibility.Visible);
        }
    }
}