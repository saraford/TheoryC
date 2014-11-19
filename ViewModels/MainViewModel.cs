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

        Point trackCenter = new Point(Settings.Default.TrackLeftX + Settings.Default.TrackRadius, Settings.Default.TrackTopY + Settings.Default.TrackRadius);

        DispatcherTimer gameTimer;

        #endregion

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

                // set the current trial so user sees it on first launch
                this.CurrentTrial = this.Trials.First();

                // setup the track
                TrackRadius = Settings.Default.TrackRadius;

                // setup the target
                this.UpdateTargetSizeAndPlaceInStartingPosition();
            }
        }

        // called by the Main Window on Loaded event
        public void Startup()
        {
            // To be eventually replaced by the researcher uploading a file or something
            for (int i = 0; i < 3; i++)
            {
                this.Trials.Add(new Models.Trial
                {
                    Number = i, // use a converter + 1, // no 0-based trials exposed to user
                    Results = new Models.Result { 
                            TimeOnTargetMs = 0, 
                            AbsoluteError = 0, 
                            AbsoluteErrorForEachTickList = new List<double>(), 
                            IsInsideTrackForEachTickList = new List<bool>() }
                });

                // to see real-time updates
                this.Trials[i].PropertyChanged += CurrentTrial_PropertyChanged;
            }

            // initialize the game timer
            gameTimer = new DispatcherTimer(DispatcherPriority.Normal);
            gameTimer.Interval = TimeSpan.FromMilliseconds(Settings.Default.MillisecondDelay);
            gameTimer.Tick += GameTimer_Tick;

            // set the current trial so user sees it on first launch
            this.CurrentTrial = this.Trials.First();

            // setup the track
            TrackRadius = Settings.Default.TrackRadius;

            // setup the target
            this.UpdateTargetSizeAndPlaceInStartingPosition();
        }

        // if the user changes the target radius from Settings, we need code to place it at right location
        void CurrentTrial_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShapeSizeDiameter")
            {
                this.PlaceTargetInStartingPosition();
            }
        }

        public void UpdateTargetSizeAndPlaceInStartingPosition()
        {
            TargetSizeRadius = CurrentTrial.ShapeSizeDiameter / 2.0;
            this.PlaceTargetInStartingPosition();
        }

        private void InitializeExperiment()
        {
            this.IsExperimentRunning = true;
            CurrentTrial = Trials.First();
            AngleInDegrees = 0;

            // clear prior results
            foreach (var trial in Trials)
            {
                trial.ClearResults();
            }
        }

        private void StopExperiment()
        {
            this.IsExperimentRunning = false;
            ResetTargetValues();
            // kepeing target wherever it is because exp has stopped
        }

        private void UpdateSceneForNextTrial()
        {
            ResetTargetValues();
            UpdateTargetSizeAndPlaceInStartingPosition();
        }

        private void ResetTargetValues()
        {
            IsOnTarget = false;
            xt = 0;
            yt = 0;
        }

        DateTime stopTime;
        Stopwatch timeOnTarget = new Stopwatch();
        Stopwatch totalTrialTime = new Stopwatch();
        double secondsToDoOneRotation;

        private void StartNextTrial()
        {
            stopTime = DateTime.Now.AddSeconds(CurrentTrial.DurationSeconds);

            timeOnTarget.Reset();
            totalTrialTime.Reset();

            TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;

            secondsToDoOneRotation = 1 / (CurrentTrial.RPMs / 60.0);

            // rock and roll
            Debug.Print("Trial #" + CurrentTrial.Number.ToString() + " starting at " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            totalTrialTime.Start();
            gameTimer.Start();

            tickCounter = 0; // for debugging
        }

        private void StopCurrentTrial()
        {
            totalTrialTime.Stop();
            gameTimer.Stop();

            Debug.Print("Trial #" + CurrentTrial.Number.ToString() + " stopped at " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            Debug.Print("Trial time took " + totalTrialTime.Elapsed.ToString());

            // save results
            CurrentTrial.Results.TimeOnTargetMs = timeOnTarget.ElapsedMilliseconds;
            CurrentTrial.Results.AbsoluteError = Statistics.Mean(CurrentTrial.Results.AbsoluteErrorForEachTickList);
            CurrentTrial.Results.ConstantError = Statistics.CalculateConstantError(CurrentTrial.Results.AbsoluteErrorForEachTickList, CurrentTrial.Results.IsInsideTrackForEachTickList);

            // Check whether to end the experiment
            if (CurrentTrial.Number + 1 >= Trials.Count)
            {
                StopExperiment();
            }
            else
            {
                // otherwise get next trial
                int nextTrialNumber = CurrentTrial.Number + 1;
                CurrentTrial = Trials[nextTrialNumber];

                // setup for next trial
                this.UpdateSceneForNextTrial();
            }
        }

        public int tickCounter = 0;
        void GameTimer_Tick(object sender, EventArgs e)
        {

            Debug.Print("Tick #" + tickCounter++ + " and time is right now " + DateTime.Now.ToString("hh.mm.ss.ffffff"));

            CalculateAngleBasedOnTimeSampling();

            MoveTargetOnTick();

            CheckIsOnTarget();

            CheckIsInsideTrackCircle(); //to calculate algebraic error

            CalculateAbsoluteErrorForEachTick();

            if (HasTrialTimeExpired())
            {
                StopCurrentTrial();
            }
        }

        private void CalculateAngleBasedOnTimeSampling()
        {
            double wp = totalTrialTime.ElapsedMilliseconds / (secondsToDoOneRotation * 1000);
            AngleInDegrees = 360.0 * wp; 
        }

        double distanceFromCenterOnTick;
        private void CalculateAbsoluteErrorForEachTick()
        {
            distanceFromCenterOnTick = Statistics.DistanceBetween2Points(this.MousePosition, this.TargetPositionCenter);
            CurrentTrial.Results.AbsoluteErrorForEachTickList.Add(distanceFromCenterOnTick);
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
            IsOnTarget = Tools.IsInsideCircle(this.MousePosition, this.TargetPositionCenter, TargetSizeRadius);

            if (IsOnTarget)
            {
                timeOnTarget.Start();
            }
            else
            {
                timeOnTarget.Stop();
            }
        }

        bool isInsideTrackCircle;
        private void CheckIsInsideTrackCircle()
        {
            // calculate whether inside the track for algebraic error
            isInsideTrackCircle = Tools.IsInsideCircle(this.MousePosition, this.trackCenter, this.TrackRadius);

            // save boolean
            CurrentTrial.Results.IsInsideTrackForEachTickList.Add(isInsideTrackCircle);
        }

        #region Commands

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

            // regardless which trial number the user left selected in the Settings window,
            // put it back to the first trial
            this.CurrentTrial = this.Trials.First();

            // update scene
            this.UpdateTargetSizeAndPlaceInStartingPosition();
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

        DelegateCommand _UpdateSceneWhenListboxSelectionChanges = null;
        public DelegateCommand UpdateSceneWhenListboxSelectionChanges
        {
            get
            {
                if (_UpdateSceneWhenListboxSelectionChanges != null)
                    return _UpdateSceneWhenListboxSelectionChanges;

                _UpdateSceneWhenListboxSelectionChanges = new DelegateCommand(new Action(PlaceTargetInStartingPosition), new Func<bool>(UpdateSceneWhenListboxSelectionChangesCanExecute));
                this.PropertyChanged += (s, e) => _UpdateSceneWhenListboxSelectionChanges.RaiseCanExecuteChanged();
                return _UpdateSceneWhenListboxSelectionChanges;
            }
        }

        public bool UpdateSceneWhenListboxSelectionChangesCanExecute()
        {
            // oh good lord, this fires even if the user did not invoke the action
            // this command should only execute if the SettingsWindow is opened
            return SetupWindowOpen;
        }

        DelegateCommand _UpdateTargetSizeRadius = null;
        public DelegateCommand UpdateTargetSizeRadius
        {
            get
            {
                if (_UpdateTargetSizeRadius != null)
                    return _UpdateTargetSizeRadius;

                _UpdateTargetSizeRadius = new DelegateCommand(new Action(UpdateTargetSizeAndPlaceInStartingPosition), new Func<bool>(UpdateTargetSizeCanExecute));
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

        DelegateCommand _StartExpCommand = null;
        public DelegateCommand StartExpCommand
        {
            get
            {
                if (_StartExpCommand != null)
                    return _StartExpCommand;

                _StartExpCommand = new DelegateCommand(new Action(StartExperiment), new Func<bool>(StartExpCanExecute));
                this.PropertyChanged += (s, e) => _StartExpCommand.RaiseCanExecuteChanged();
                return _StartExpCommand;
            }
        }

        public bool StartExpCanExecute()
        {
            return true;
        }

        public void StartExperiment()
        {
            if (!IsExperimentRunning)
            {
                InitializeExperiment();
                StartNextTrial();
            }
            else
            {
                StopCurrentTrial();
                StopExperiment();
            }
        }

        DelegateCommand _StartTrialCommand = null;
        public DelegateCommand StartTrialCommand
        {
            get
            {
                if (_StartTrialCommand != null)
                    return _StartTrialCommand;

                _StartTrialCommand = new DelegateCommand(new Action(StartTrialExecuted), new Func<bool>(StartTrialCanExecute));
                this.PropertyChanged += (s, e) => _StartTrialCommand.RaiseCanExecuteChanged();
                return _StartTrialCommand;
            }
        }

        public bool StartTrialCanExecute()
        {
            // can only run if the overall experiment is running
            return IsExperimentRunning;
        }

        public void StartTrialExecuted()
        {
            StartNextTrial();
        }

        #endregion

    }
}
