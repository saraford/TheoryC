/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TheoryC.Converters
{
    class RoundValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double number = (double)value;
            return Math.Round(number, 5);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
