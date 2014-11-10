using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TheoryC.Common;
using TheoryC.Properties;

namespace TheoryC.ViewModels
{
    public class MainViewModel : Common.BindableBase
    {

        #region Bindable Properties

        ObservableCollection<Models.Trial> _Trials = new ObservableCollection<Models.Trial>();
        public ObservableCollection<Models.Trial> Trials { get { return _Trials; } }

        Models.Trial _currentTrial = default(Models.Trial);
        public Models.Trial CurrentTrial { get { return _currentTrial; } set { base.SetProperty(ref _currentTrial, value); } }

        Point _MousePosition = default(Point);
        public Point MousePosition { get { return _MousePosition; } set { base.SetProperty(ref _MousePosition, value); } }

        bool _IsRunning = default(bool);
        public bool IsRunning { get { return _IsRunning; } set { base.SetProperty(ref _IsRunning, value); } }

        double _angle = default(double);
        public double Angle { get { return _angle; } set { base.SetProperty(ref _angle, value); } }

        bool _IsOnTarget = default(bool);
        public bool IsOnTarget { get { return _IsOnTarget; } set { base.SetProperty(ref _IsOnTarget, value); } }

        double _TargetPositionLeft = default(double);
        public double TargetPositionLeft { get { return _TargetPositionLeft; } set { base.SetProperty(ref _TargetPositionLeft, value); } }

        double _TargetPositionTop = default(double);
        public double TargetPositionTop { get { return _TargetPositionTop; } set { base.SetProperty(ref _TargetPositionTop, value); } }

        Point _TargetPositionCenter = default(Point);
        public Point TargetPositionCenter
        {
            get
            {
                _TargetPositionCenter.X = TargetPositionLeft + (this.CurrentTrial.ShapeSizeDiameter / 2.0);
                _TargetPositionCenter.Y = TargetPositionTop + (this.CurrentTrial.ShapeSizeDiameter / 2.0);
                return _TargetPositionCenter;
            }
        }

        #endregion

        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Settings.Default.MillisecondDelay) };
        Point trackCenter = new Point(Settings.Default.TrackCenterX, Settings.Default.TrackCenterY);
        double trackRadius = Settings.Default.TrackRadius;
        double TargetSizeRadius;
        public List<double> absErrorDuringTrial = new List<double>();

        public MainViewModel()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                // some pretend results
                // To be eventually replaced by the researcher uploading a file or something
                for (int i = 0; i < 3; i++)
                {
                    this.Trials.Add(new Models.Trial
                    {
                        Number = i + 1, // no 0-based trials exposed to user
                        Results = new Models.Result { TimeOnTargetMS = 10, AbsoluteError = 3.5 }
                    });
                }

                this.CurrentTrial = this.Trials.First();
            }
        }

        // called by the Main Window on Loaded event
        internal void Startup()
        {
            // To be eventually replaced by the researcher uploading a file or something
            for (int i = 0; i < 3; i++)
            {
                this.Trials.Add(new Models.Trial
                {
                    Number = i + 1, // no 0-based trials exposed to user
                    Results = new Models.Result { TimeOnTargetMS = 10, AbsoluteError = 3.5 }
                });
            }

            this.CurrentTrial = this.Trials.First();
            timer.Tick += Timer_Tick;
            CurrentTrial.PropertyChanged += CurrentTrial_PropertyChanged;
            PutTargetOnTrack();
        }

        void CurrentTrial_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if the shape's size is changed, we need to place it at the right spot
            if (e.PropertyName == "ShapeSizeDiameter")
            {
                this.PutTargetOnTrack();
            }
        }

        bool _SetupWindowOpen = default(bool);
        public bool SetupWindowOpen { get { return _SetupWindowOpen; } set { base.SetProperty(ref _SetupWindowOpen, value); } }

        DelegateCommand _ShowSettingsCommand = null;
        public DelegateCommand ShowSettingsCommand
        {
            get
            {
                if (_ShowSettingsCommand != null)
                    return _ShowSettingsCommand;

                _ShowSettingsCommand = new DelegateCommand
                (
                    () =>
                    {
                        var settings = new Views.SettingsWindow(SettingsWindowSetupCallback);
                        settings.DataContext = this; // to share same model data

                        SetupWindowOpen = true;
                        settings.Show();
                    },
                    () =>
                    {
                        return !SetupWindowOpen;
                    }
                );
                this.PropertyChanged += (s, e) => _ShowSettingsCommand.RaiseCanExecuteChanged();
                return _ShowSettingsCommand;
            }
        }

        private void SettingsWindowSetupCallback(MainViewModel viewmodel)
        {
            SetupWindowOpen = false;
        }

        bool _DebugWindowOpen = default(bool);
        public bool DebugWindowOpen { get { return _DebugWindowOpen; } set { base.SetProperty(ref _DebugWindowOpen, value); } }

        DelegateCommand _ShowDebugCommand = null;
        public DelegateCommand ShowDebugCommand
        {
            get
            {
                if (_ShowDebugCommand != null)
                    return _ShowDebugCommand;

                _ShowDebugCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (!DebugWindowOpen)
                        {
                            DebugWindowOpen = true;

                            var debugWin = new Views.DebugWindow();
                            debugWin.DataContext = this; // to share same model data
                            var main = Application.Current.MainWindow;
                            debugWin.Left = main.Left;
                            debugWin.Top = main.Top + main.Height;
                            debugWin.Owner = main; // whatever happens to main window happens here
                            debugWin.ShowInTaskbar = false;
                            debugWin.Show();
                        }
                        else
                        {
                            DebugWindowOpen = false;

                            var debugWin = Application.Current.Windows.OfType<Views.DebugWindow>().First();
                            debugWin.Close();
                        }
                    },
                    () =>
                    {
                        return true; // always have debug command enabled
                    }
                );
                this.PropertyChanged += (s, e) => _ShowDebugCommand.RaiseCanExecuteChanged();
                return _ShowDebugCommand;
            }
        }

        DelegateCommand _StartCommand = null;
        public DelegateCommand StartCommand
        {
            get
            {
                if (_StartCommand != null)
                    return _StartCommand;

                _StartCommand = new DelegateCommand(new Action(StartExecuted), new Func<bool>(StartCanExecute));
                this.PropertyChanged += (s, e) => _StartCommand.RaiseCanExecuteChanged();
                return _StartCommand;
            }
        }

        public bool StartCanExecute()
        {
            return true;
            //  return !IsRunning; //only when not running can you start the experiment
        }

        DateTime stopTime;
        public void StartExecuted()
        {
            if (!IsRunning)
            {

                stopTime = DateTime.Now.AddSeconds(CurrentTrial.DurationSeconds);
                //tickCounter = 0;
                // set the radius so we don't have to do /2 every tick
                TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;
                timer.Start();
                this.IsRunning = true;
            }
            else
            {
                StopTimer();
            }
        }


        public int tickCounter = 0;
        double xt, yt;
        void Timer_Tick(object sender, EventArgs e)
        {
            //                        Debug.Print("Tick #" + tickCounter++ + " at " + DateTime.Now);

            Angle += .07; // TODO: Figure out the speed here

            MoveTargetOnTick();

            CheckElapsedTime();

            CheckIsOnTarget();

            GetAbsoluteErrorForTick();
        }

        private void MoveTargetOnTick()
        {
            Common.Tools.PointsOnACircle(trackRadius, Angle, trackCenter, out xt, out yt);
            TargetPositionLeft = xt - TargetSizeRadius;
            TargetPositionTop = yt - TargetSizeRadius;
        }

        private void CheckElapsedTime()
        {
            int result = DateTime.Compare(DateTime.Now, stopTime);
            if (result > 0)
            {
                StopTimer();
            }
        }

        private void PutTargetOnTrack()
        {
            Point pt = Tools.GetPointForPlacingTargetOnStartTrack(trackCenter, trackRadius, CurrentTrial.ShapeSizeDiameter / 2.0);

            TargetPositionLeft = pt.X;
            TargetPositionTop = pt.Y;
        }

        private void CheckIsOnTarget()
        {
            // calculate whether inside the circle
            // IsOnTarget is a DP that goes through a converter to set the color of the shape  
            IsOnTarget = Tools.IsInsideCircle(this.MousePosition, this.TargetPositionCenter, (this.CurrentTrial.ShapeSizeDiameter / 2.0));
        }

        double distanceFromCenterOnTick;
        private void GetAbsoluteErrorForTick()
        {
            // for entire trials               
            //            double distanceFromCenterOnTick = Statistics.DistanceBetween2Points(this.MousePosition, this.TargetPositionCenter);            
            //            this.absErrorDuringTrial.Add(distanceFromCenterOnTick);
            //            CurrentTrial.Results.AbsoluteError = Statistics.StandardDeviation(absErrorDuringTrial);

            // just showing for each tick
            CurrentTrial.Results.AbsoluteError = Statistics.DistanceBetween2Points(this.MousePosition, this.TargetPositionCenter);
        }

        private void StopTimer()
        {
            //            Debug.Print("Stopping at " + DateTimeOffset.Now.ToString());
            timer.Stop();
            this.IsRunning = false;

        }

        // called by the MainWindow when shutting down
        internal void Shutdown()
        {
            //if (debugWin != null)
            //    debugWin.Close();
        }
    }
}
