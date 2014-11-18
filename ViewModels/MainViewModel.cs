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

        double _angleInDegrees = default(double);
        public double AngleInDegrees { get { return _angleInDegrees; } set { base.SetProperty(ref _angleInDegrees, value); } }

        bool _IsOnTarget = default(bool);
        public bool IsOnTarget { get { return _IsOnTarget; } set { base.SetProperty(ref _IsOnTarget, value); } }

        // needed to avoid the flicker when sizes change during trial
        double _TargetSizeRadius = default(double);
        public double TargetSizeRadius { get { return _TargetSizeRadius; } set { base.SetProperty(ref _TargetSizeRadius, value); } }

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

        double _TrackRadius = default(double);
        public double TrackRadius { get { return _TrackRadius; } set { base.SetProperty(ref _TrackRadius, value); } }

        #endregion

        DispatcherTimer timer;
        Point trackCenter = new Point(Settings.Default.TrackLeftX + Settings.Default.TrackRadius, Settings.Default.TrackTopY + Settings.Default.TrackRadius);

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
                        Results = new Models.Result { TimeOnTargetMs = 0, AbsoluteError = 0, AbsoluteErrorForEachTickList = new List<double>() }
                    });
                }

                this.CurrentTrial = this.Trials.First();
                TrackRadius = Settings.Default.TrackRadius;
                TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;
                this.PlaceTargetInStartingPosition();
            }
        }

        // called by the Main Window on Loaded event
        public void Startup()
        {
            // To be eventually replaced by the researcher uploading a file or something
            for (int i = 0; i < 1; i++)
            {
                this.Trials.Add(new Models.Trial
                {
                    Number = i, // use a converter + 1, // no 0-based trials exposed to user
                    Results = new Models.Result { TimeOnTargetMs = 0, AbsoluteError = 0, AbsoluteErrorForEachTickList = new List<double>() }                
                });

                // to see real-time updates
                this.Trials[i].PropertyChanged += CurrentTrial_PropertyChanged;
            }

            this.CurrentTrial = this.Trials.First();

            timer = new DispatcherTimer(DispatcherPriority.Normal);
            timer.Interval = TimeSpan.FromMilliseconds(Settings.Default.MillisecondDelay);

            timer.Tick += Timer_Tick;

            TrackRadius = Settings.Default.TrackRadius;
            TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;

            PlaceTargetInStartingPosition();

            this.TrialCompleted += StartNextTrial_TrialCompleted;
        }

        void CurrentTrial_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if the shape's size is changed, we need to place it at the right spot
            if (e.PropertyName == "ShapeSizeDiameter")
            {
                this.PlaceTargetInStartingPosition();
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
            this.CurrentTrial = this.Trials.First();
            TargetSizeRadius = CurrentTrial.ShapeSizeDiameter / 2.0;
            this.PlaceTargetInStartingPosition();
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

        internal void ShowDebugWindow()
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

        DelegateCommand _UpdateScene = null;
        public DelegateCommand UpdateScene
        {
            get
            {
                if (_UpdateScene != null)
                    return _UpdateScene;

                _UpdateScene = new DelegateCommand(new Action(RefreshScene), new Func<bool>(UpdateSceneCanExecute));
                this.PropertyChanged += (s, e) => _UpdateScene.RaiseCanExecuteChanged();
                return _UpdateScene;
            }
        }

        public bool UpdateSceneCanExecute()
        {
            // oh good lord, this fires even if the user did not invoke the action
            // this command should only execute if the SettingsWindow is opened
            return SetupWindowOpen;
        }

        public void RefreshScene()
        {
            this.PlaceTargetInStartingPosition();
        }

        DelegateCommand _UpdateTargetSizeRadius = null;
        public DelegateCommand UpdateTargetSizeRadius
        {
            get
            {
                if (_UpdateTargetSizeRadius != null)
                    return _UpdateTargetSizeRadius;

                _UpdateTargetSizeRadius = new DelegateCommand(new Action(UpdateTargetSize), new Func<bool>(UpdateTargetSizeCanExecute));
                this.PropertyChanged += (s, e) => _UpdateTargetSizeRadius.RaiseCanExecuteChanged();
                return _UpdateTargetSizeRadius;
            }
        }

        public bool UpdateTargetSizeCanExecute()
        {
            // oh good lord, this fires even if the user did not invoke the action
            // this command should only execute if the SettingsWindow is opened
            return SetupWindowOpen;
        }

        public void UpdateTargetSize()
        {
            TargetSizeRadius = CurrentTrial.ShapeSizeDiameter / 2.0;
            this.PlaceTargetInStartingPosition();
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
                // reset results / data
                InitializeExperiment();
                StartNextTrial();
            }
            else
            {
                StopCurrentTrial(goToNextTrial:false);
                StopExperiment();
            }
        }

        private void InitializeExperiment()
        {
            CurrentTrial = Trials.First();

            // clear prior results
            foreach (var trial in Trials)
            {
                trial.ClearResults();
            }
        }

        private void StopExperiment()
        {
            ResetScene();
        }

        private void ResetScene()
        {
            this.PlaceTargetInStartingPosition();
            this.AngleInDegrees = 0;
            IsOnTarget = false;
            xt = 0;
            yt = 0;
        }

        // create a test that verifies all Results and values are reset
        Stopwatch timeOnTarget = new Stopwatch();
        Stopwatch totalTrialTime = new Stopwatch();
        private void StartNextTrial()
        {
            stopTime = DateTime.Now.AddSeconds(CurrentTrial.DurationSeconds);

            // must add an extra tick
            //  stopTime = stopTime.AddMilliseconds(Settings.Default.MillisecondDelay);
            timeOnTarget.Reset(); 
            totalTrialTime.Reset();

            // this is needed to avoid flickering because of the data binding to the 
            // CurrentTrial's ShapeSize. We need to place its position first and then resize
            // the other way around causes flickering
            if (CurrentTrial.Number != 0)
            {
                double dumbTargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;
                TargetPositionLeft = xt - dumbTargetSizeRadius;
                TargetPositionTop = yt - dumbTargetSizeRadius;
                TargetSizeRadius = dumbTargetSizeRadius;
            }

            // rock and roll
            Debug.Print("Trial #" + CurrentTrial.Number.ToString() + " starting at " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            totalTrialTime.Start();
            timer.Start();
            this.IsExperimentRunning = true;

            tickCounter = 0; // for debugging
        }

        private void StopCurrentTrial(bool goToNextTrial)
        {
            totalTrialTime.Stop();
            timer.Stop();
            CurrentTrial.Results.TimeOnTargetMs = timeOnTarget.ElapsedMilliseconds;
            Debug.Print("Trial #" + CurrentTrial.Number.ToString() + " stopped at " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            Debug.Print("Trial time took " + totalTrialTime.Elapsed.ToString());
            this.IsExperimentRunning = false;

            // fire the event to call the next trial
            if (goToNextTrial)
            {
                OnTrialCompleted(new EventArgs());
            }
        }

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

        public event EventHandler TrialCompleted;

        protected virtual void OnTrialCompleted(EventArgs e)
        {
            if (TrialCompleted != null)
            {
                TrialCompleted(this, e);
            }
        }

        public int tickCounter = 0;
        void Timer_Tick(object sender, EventArgs e)
        {

            Debug.Print("Tick #" + tickCounter++ + " and time is right now " + DateTime.Now.ToString("hh.mm.ss.ffffff"));

            CalculateAngleBasedOnTimeSampling();

            MoveTargetOnTick();

            CheckIsOnTarget();

            UpdateAbsoluteErrorForTrial();

            if (HasTrialTimeExpired())
            {
                StopCurrentTrial(goToNextTrial:true);
            }
        }

        private void CalculateAngleBasedOnTimeSampling()
        {
            // wp% of rotationTime = elapsed time
            // wp% of 360 = current angle
            double wp = totalTrialTime.ElapsedMilliseconds / (CurrentTrial.RotationSpeedInSeconds * 1000);
            AngleInDegrees = 360.0 * wp;
            //            Debug.Print("Angle is " + AngleInDegrees);
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

        double xt, yt;
        private void MoveTargetOnTick()
        {
            Common.Tools.PointsOnACircle(TrackRadius, AngleInDegrees, trackCenter, out xt, out yt);
            TargetPositionLeft = xt - TargetSizeRadius;
            TargetPositionTop = yt - TargetSizeRadius;
        }

        private bool HasTrialTimeExpired()
        {
            int result = DateTime.Compare(DateTime.Now, stopTime);
            return (result > 0); // true if time has expired
        }

        private void PlaceTargetInStartingPosition()
        {
            Point pt = Tools.GetPointForPlacingTargetInStartingPosition(trackCenter, TrackRadius, TargetSizeRadius);

            TargetPositionLeft = pt.X;
            TargetPositionTop = pt.Y;
        }

        private void CheckIsOnTarget()
        {
            // calculate whether inside the circle
            // IsOnTarget is a DP that goes through a converter to set the color of the shape  
            IsOnTarget = Tools.IsInsideCircle(this.MousePosition, this.TargetPositionCenter, TargetSizeRadius);

            if (IsOnTarget)
            {
                timeOnTarget.Start();
                //                CurrentTrial.Results.TimeOnTargetMs += Settings.Default.MillisecondDelay;
            }
            else
            {
                timeOnTarget.Stop();
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
