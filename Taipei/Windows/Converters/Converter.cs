using System;
using System.Globalization;
using System.Windows.Data;

namespace Taipei.Windows.Converters
{
    // Reuses HasError (bool?) to control visibility
    public class NullableBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // Formats the unix-epoch-style decimal timestamps (req_start, resp_end, etc.)
    public class UnixDecimalToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal seconds)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)(seconds * 1000)).LocalDateTime;
                return dt.ToString("HH:mm:ss.fff");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}