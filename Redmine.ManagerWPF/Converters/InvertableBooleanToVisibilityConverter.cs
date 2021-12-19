using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
