using System.Globalization;

namespace MauiApp1.Converters
{
    public class TimeColorConverter : Microsoft.Maui.Controls.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.TotalMinutes <= 1) // Son 1 dakika
                    return Colors.Red;
                if (timeSpan.TotalMinutes <= 5) // Son 5 dakika
                    return Colors.Orange;
                return Colors.Green;
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 