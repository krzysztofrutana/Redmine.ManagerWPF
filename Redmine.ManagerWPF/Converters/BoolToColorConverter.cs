using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Data;

namespace Redmine.ManagerWPF.Desktop.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((bool?)value) == true)
                return Brushes.Green;
            else if (((bool?)value) == false)
                return Brushes.Red;
            else
                return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // no need to implement it
            throw new NotImplementedException();
        }
    }
}