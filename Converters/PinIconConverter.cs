using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf; // For PackIconKind

namespace RuleArchitectPrototype.Converters
{
    public class PinIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isPinned)
            {
                // Default: Pin (when unpinned), PinOff (when pinned)
                PackIconKind unpinnedIcon = PackIconKind.PinOutline;
                PackIconKind pinnedIcon = PackIconKind.PinOffOutline; // Or Pin if you want it to look "active"

                if (parameter is string kinds) // e.g., "Pin/PinOff"
                {
                    var parts = kinds.Split('/');
                    if (parts.Length == 2)
                    {
                        Enum.TryParse(parts[0], out pinnedIcon);
                        Enum.TryParse(parts[1], out unpinnedIcon);
                    }
                }
                return isPinned ? pinnedIcon : unpinnedIcon;
            }
            return PackIconKind.HelpCircle; // Default fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}