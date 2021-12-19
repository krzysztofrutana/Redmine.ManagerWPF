using Redmine.ManagerWPF.Data.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.Converters
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class IsEverythingElseThanTimeIntervalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var type = (string)value;
            if (type != ObjectType.TimeInterval.ToString())
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
