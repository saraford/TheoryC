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
            Target.MouseLeftButtonUp += Target_MouseLeftButtonUp;
        }

        void Target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCoordinatesToTopLeftCorner();

            this.ViewModel.Startup();
            this.ViewModel.ShowDebugWindow();

            GetLocationOfTargetForSettingMouse();
        }

        void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.ViewModel.MousePosition = e.GetPosition(this);
        }

        // only to be used to set the mouse position
        public void GetLocationOfTargetForSettingMouse()
        {
            this.ViewModel.AbsoluteScreenPositionOfTarget = Target.PointToScreen(new Point(0, 0));
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
