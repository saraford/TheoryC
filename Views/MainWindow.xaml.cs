using System;
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
            this.ResizeMode = System.Windows.ResizeMode.NoResize;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetCoordinatesToTopLeftCorner();

            this.ViewModel.Startup();

            DetermineUserInputModeType();

            this.ViewModel.ShowSettingsOnLaunch();
        }

        // The reason this code is here in the View is becuase of the MouseMove event handler
        // I wanted to keep it simple and have the View determine whether to hook up the MouseMove event
        private void DetermineUserInputModeType()
        {
            // Note: I originally did a 2 second delay here to automatically detect a kinect but
            // I didn't like 1. having it hard coded for all of time and 2. the spinner on launch for mouse mode
            // thus i decided to prompt the user to have a bit more control and much more responsive UI in mouse mode

            // using a WPF Message box to keep things simple
            // unfortunately, you can't customize a WPF Message box and I don't want to use the ViewModel (more work than worth it)      
            // so this text will have to do
            if (MessageBox.Show("Do you want to use a Kinect? \n\nClick Yes for Kinect mode. Click No for Mouse mode.", "TheoryC in Kinect or Mouse Mode?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // User requested Kinect mode
                myKinect = new Devices.KinectDevice();

                bool keepTrying = true;
                DateTime stoptime = DateTime.Now.AddSeconds(2);

                // try to find the kinect sensor for 2 seconds
                // I've noticed that there's about a 1 second delay between kinectSensor.Open() and kinectSensor.IsAvailable
                // so we got to keep trying for 1-2 seconds. 
                do
                {
                    this.ViewModel.IsUsingKinect = myKinect.CheckIsKinectAvailable();

                    if (DateTime.Now > stoptime)
                    {
                        keepTrying = false;
                    }
                                        
                } while (!this.ViewModel.IsUsingKinect && keepTrying);

                // Verify we were able to detect the sensor
                if (this.ViewModel.IsUsingKinect)
                {
                    myKinect.InitializeKinect(bodyCanvas, kinectVideoImage, ViewModel);
                }
                else
                {
                    // if failed to get a kinect, prompt user
                    MessageBox.Show("Unable to detect a kinect sensor. Defaulting to Mouse Mode.");
                    MouseMove += MainWindow_MouseMove;
                }
            }
            else
            {
                // User requested Mouse Mode
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (myKinect != null)
            {
                myKinect.Shutdown();
            }
        }
    }
}
