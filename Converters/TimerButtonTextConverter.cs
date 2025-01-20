using System;
using System.Globalization;

namespace MauiApp1.Converters
{
    public class TimerButtonTextConverter : Microsoft.Maui.Controls.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "⏸️" : "▶️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 