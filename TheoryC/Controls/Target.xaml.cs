/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TheoryC.Controls
{
    /// <summary>
    /// Interaction logic for Target.xaml
    /// </summary>
    public partial class Target : UserControl
    {
        public Target()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;

        }

        private static DependencyProperty FillColorProperty = DependencyProperty.Register("FillColor", typeof(Brush), typeof(Target), null);

        public Brush FillColor
        {
            get { return (Brush)GetValue(FillColorProperty) as Brush; }
            set { SetValue(FillColorProperty, value); }
        }
    }
}
