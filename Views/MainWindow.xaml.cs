using System.Windows;
using System.Windows.Input;

namespace TheoryC.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MouseMove += MainWindow_MouseMove;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCoordinatesToTopLeftCorner();

            this.ViewModel.Startup();
            this.ViewModel.ShowDebugWindow();
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.ViewModel.Shutdown();
        }

        void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.ViewModel.MousePosition = e.GetPosition(this);
        }

        public ViewModels.MainViewModel ViewModel
        {
            get { return this.DataContext as ViewModels.MainViewModel; }
        }

        private void SetCoordinatesToTopLeftCorner()
        {
            var desktop = System.Windows.SystemParameters.WorkArea;
            this.Left = desktop.Left;
            this.Top = desktop.Top;
        }
    }
}
