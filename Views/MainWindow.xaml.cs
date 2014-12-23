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
                // yeah, i know I'm being lazy here using a message box
                MessageBox.Show("kinect not detected. using mouse mode");
                MouseMove += MainWindow_MouseMove;
            }
        }


        void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            this.ViewModel.InputPosition = e.GetPosition(SceneCanvas);

            // Uncomment to use for any mouse debugging
            //            this.ViewModel.MousePosition = e.GetPosition(SceneCanvas);
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
