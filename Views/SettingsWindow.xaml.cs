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
using System.Windows.Shapes;

namespace TheoryC.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private Action _closing;
        public SettingsWindow(Action closing)
        {
            InitializeComponent();

            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            Closing += Setup_Closing;
            _closing = closing;
        }

        void Setup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing.Invoke();
        }

    }
}
