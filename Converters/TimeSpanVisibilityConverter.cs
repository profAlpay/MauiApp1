using System;
using System.Globalization;

namespace MauiApp1.Converters
{
    public class TimeSpanVisibilityConverter : Microsoft.Maui.Controls.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan > TimeSpan.Zero;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 