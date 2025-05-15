using System;
using System.Globalization;
using System.Windows.Data;

namespace RuleArchitectPrototype.Converters
{
    public class PinTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
            {
                string unpinnedText = "Pin Tab";
                string pinnedText = "Unpin Tab";

                if (parameter is string texts) // e.g., "Pin Tab/Unpin Tab"
                {
                    var parts = texts.Split('/');
                    if (parts.Length == 2)
                    {
                        pinnedText = parts[0];    // Text when IsPinned = true
                        unpinnedText = parts[1];  // Text when IsPinned = false
                    }
                }
                return isPinned ? pinnedText : unpinnedText;
            }
            return "Pin/Unpin";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}