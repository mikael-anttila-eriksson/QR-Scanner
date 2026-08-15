using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace UIApp.Converters
{
    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                var ts = DateTime.UtcNow - dt.ToUniversalTime();
                if (ts.TotalSeconds < 60) return "just now";
                if (ts.TotalMinutes < 60) return $"{(int)ts.TotalMinutes}m ago";
                if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h ago";
                if (ts.TotalDays < 7) return $"{(int)ts.TotalDays}d ago";
                return dt.ToLocalTime().ToString("g");
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}