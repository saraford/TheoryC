/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

// 2nd answer to http://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum 
namespace TheoryC.Converters
{
    public class RealityEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }    
}
