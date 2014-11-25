using System.Windows;
using System.Windows.Input;

namespace TheoryC.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Devices.KinectDevice myKinect;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCoordinatesToTopLeftCorner();

            this.ViewModel.Startup();

            DetermineUserInputModeType();
        }

        private void DetermineUserInputModeType()
        {
            myKinect = new Devices.KinectDevice();
            this.ViewModel.IsUsingKinect = myKinect.IsKinectAvailable();

            if (this.ViewModel.IsUsingKinect)
            {
                myKinect.InitializeKinect(bodyCanvas, kinectVideoImage, ViewModel);
            }
            else
            {
                // yeah, i know I'm being lazy here
                MessageBox.Show("kinect not detected. using mouse mode");
                MouseMove += MainWindow_MouseMove;

                // REMOVE THIS ONCE I FIGURE OUT KINECT SCALING ISSUES
                // gameCanvas and kinectVideo must be both at Width="960" Height="510"
                //                this.MainWindowGrid.Height = 600;
                //                this.gameCanvas.Height = 600;
            }
        }


        void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.ViewModel.InputPosition = e.GetPosition(SceneCanvas);

            // use for any mouse debugging
            //            this.ViewModel.MousePosition = e.GetPosition(SceneCanvas);
        }

//        // NO IDEA HOW TO CALL THIS
//        // only to be used to set the mouse position
//        public void GetLocationOfTargetForSettingMouse()
//        {
//            throw new System.NotImplementedException("Don't know how to get the view to call me. But you can call me Al");
////            this.ViewModel.AbsoluteScreenPositionOfTarget = Target.PointToScreen(new Point(0, 0));
//        }

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
