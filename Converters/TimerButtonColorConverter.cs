using System;
using System.Globalization;

namespace MauiApp1.Converters
{
    public class TimerButtonColorConverter : Microsoft.Maui.Controls.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Colors.Orange : Colors.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 