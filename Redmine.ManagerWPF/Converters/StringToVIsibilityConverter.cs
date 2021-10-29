using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.Converters
{
    public class StringToVIsibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (String.IsNullOrWhiteSpace((string)value))
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