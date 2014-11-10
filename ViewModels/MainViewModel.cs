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

        bool _IsExperimentRunning = default(bool);
        public bool IsExperimentRunning { get { return _IsExperimentRunning; } set { base.SetProperty(ref _IsExperimentRunning, value); } }

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
                        Number = i, //; //use a converter + 1, // no 0-based trials exposed to user
                        Results = new Models.Result { TimeOnTargetMs = 3.5, AbsoluteError = 3.5, AbsoluteErrorForEachTickList = new List<double>() }
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
                    Number = i, // use a converter + 1, // no 0-based trials exposed to user
                    Results = new Models.Result { TimeOnTargetMs = 0, AbsoluteError = 0 , AbsoluteErrorForEachTickList = new List<double>()}
                });
            }

            this.CurrentTrial = this.Trials.First();
            timer.Tick += Timer_Tick;
            PutTargetOnTrack();

            // needed if in the future user changes something about scene, they'll get to see it in realtime
            CurrentTrial.PropertyChanged += CurrentTrial_PropertyChanged;
            this.TrialCompleted += StartNextTrial_TrialCompleted;

            // Show debug window
            ShowDebugWindow();
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
                            ShowDebugWindow();
                        }
                        else
                        {
                            HideDebugWindow();
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

        private void HideDebugWindow()
        {
            DebugWindowOpen = false;
            var debugWin = Application.Current.Windows.OfType<Views.DebugWindow>().First();
            debugWin.Close();
        }

        private void ShowDebugWindow()
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
            main.Focus(); // put focus back on main window
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
            if (!IsExperimentRunning)
            {
                StartNextTrial();

            }
            else
            {
                StopCurrentTrial();
            }
        }

        // throws an exception
        //private async void StartExperiment()
        //{
        //    foreach (var trial in Trials)
        //    {
        //        await Task.Run(() => StartTrial());
        //        Debug.Print("moving on " + DateTime.Now.ToString());                                         
        //    }
        //}

        private void StartNextTrial_TrialCompleted(object sender, EventArgs e)
        {
            // check to see if there are any trials left to run
            if (CurrentTrial.Number + 1 >= Trials.Count)
            {
                StopExperiment();
            }
            else
            {
                int nextTrialNumber = CurrentTrial.Number + 1;
                CurrentTrial = Trials[nextTrialNumber];
                StartNextTrial();
            }

        }

        private void StopExperiment()
        {
            // just let it execute for now...
        }

        private void StartNextTrial()
        {
            // is there anything to reset??
            stopTime = DateTime.Now.AddSeconds(CurrentTrial.DurationSeconds);

            // set the radius so we don't have to do /2 every tick
            TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;

            // rock and roll
            timer.Start();
            this.IsExperimentRunning = true;

            //tickCounter = 0; // for debugging
        }

        private void StopCurrentTrial()
        {
            //            Debug.Print("Stopping at " + DateTimeOffset.Now.ToString());
            timer.Stop();
            this.IsExperimentRunning = false;

            // fire the event to call the next trial
            OnTrialCompleted(new EventArgs());
        }



        public event EventHandler TrialCompleted;
        
        protected virtual void OnTrialCompleted(EventArgs e)
        {
            if (TrialCompleted != null)
            {
                TrialCompleted(this, e);
            }
        }

        public int tickCounter = 0;
        double xt, yt;
        void Timer_Tick(object sender, EventArgs e)
        {
            //                        Debug.Print("Tick #" + tickCounter++ + " at " + DateTime.Now);

            Angle += .07; // TODO: Figure out the speed here

            MoveTargetOnTick();

            CheckIsOnTarget();

            UpdateAbsoluteErrorForTrial();

            if (HasTrialTimeExpired())
            {
                StopCurrentTrial();
            }
        }

        // Hmm, how might I unit test this??
        double distanceFromCenterOnTick;
        private void UpdateAbsoluteErrorForTrial()
        {
            // for entire trials               
            distanceFromCenterOnTick = Statistics.DistanceBetween2Points(this.MousePosition, this.TargetPositionCenter);
            CurrentTrial.Results.AbsoluteErrorForEachTickList.Add(distanceFromCenterOnTick);
            CurrentTrial.Results.AbsoluteError = Statistics.StandardDeviation(CurrentTrial.Results.AbsoluteErrorForEachTickList);
        }
        
        private void MoveTargetOnTick()
        {
            Common.Tools.PointsOnACircle(trackRadius, Angle, trackCenter, out xt, out yt);
            TargetPositionLeft = xt - TargetSizeRadius;
            TargetPositionTop = yt - TargetSizeRadius;
        }

        private bool HasTrialTimeExpired()
        {
            int result = DateTime.Compare(DateTime.Now, stopTime);
            return (result > 0); // true if time has expired
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

            if (IsOnTarget)
            {
                CurrentTrial.Results.TimeOnTargetMs += Settings.Default.MillisecondDelay;
            }
        }

        // called by the MainWindow when shutting down
        internal void Shutdown()
        {
            //if (debugWin != null)
            //    debugWin.Close();
        }
    }
}
