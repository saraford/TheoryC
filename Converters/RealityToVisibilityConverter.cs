﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace TheoryC.Converters
{
    public class RealityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ViewModels.Reality original = (ViewModels.Reality)value;
            if (original == ViewModels.Reality.Augmented)
            {
                // show camera image
                return Visibility.Visible;
            }
            else
            {
                // hide camera image, we're not showing it in code iether
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception();
        }
    }
}
