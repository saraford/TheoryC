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
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        public ResultsWindow()
        {
            InitializeComponent();
        }

                private Action _closing;
        public ResultsWindow(Action closing)
        {
            InitializeComponent();

            Closing += Setup_Closing;
            _closing = closing;
        }

        void Setup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing.Invoke();
        }

    }
}
