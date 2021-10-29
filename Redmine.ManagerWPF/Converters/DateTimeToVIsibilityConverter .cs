using System;
using System.Windows;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.Converters
{
    public class DateTimeToVIsibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Hidden;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // no need to implement it
            throw new NotImplementedException();
        }
    }
}