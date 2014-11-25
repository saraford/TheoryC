using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        Point _InputPosition = default(Point);
        public Point InputPosition { get { return _InputPosition; } set { base.SetProperty(ref _InputPosition, value); } }

        // for debugging
        //Point _MousePosition = default(Point);
        //public Point MousePosition { get { return _MousePosition; } set { base.SetProperty(ref _MousePosition, value); } }

        bool _IsExperimentRunning = default(bool);
        public bool IsExperimentRunning { get { return _IsExperimentRunning; } set { base.SetProperty(ref _IsExperimentRunning, value); } }

        double _angleInDegrees = default(double);
        public double AngleInDegrees { get { return _angleInDegrees; } set { base.SetProperty(ref _angleInDegrees, value); } }

        bool _IsOnTarget = default(bool);
        public bool IsOnTarget { get { return _IsOnTarget; } set { base.SetProperty(ref _IsOnTarget, value); } }

        double _TargetPositionLeft = default(double);
        public double TargetPositionLeft { get { return _TargetPositionLeft; } set { base.SetProperty(ref _TargetPositionLeft, value); } }

        double _TargetPositionTop = default(double);
        public double TargetPositionTop { get { return _TargetPositionTop; } set { base.SetProperty(ref _TargetPositionTop, value); } }

        string _ParticipantID = default(string);
        public string ParticipantID { get { return _ParticipantID; } set { base.SetProperty(ref _ParticipantID, value); } }

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

        bool _ShowParticipantInstructions = default(bool);
        public bool ShowParticipantInstructions { get { return _ShowParticipantInstructions; } set { base.SetProperty(ref _ShowParticipantInstructions, value); } }

        bool _ShowInstructionsToStartTrial = default(bool);
        public bool ShowInstructionsToStartTrial { get { return _ShowInstructionsToStartTrial; } set { base.SetProperty(ref _ShowInstructionsToStartTrial, value); } }

        string _ShowInstructionsToStartTrialText = default(string);
        public string ShowInstructionsToStartTrialText { get { return _ShowInstructionsToStartTrialText; } set { base.SetProperty(ref _ShowInstructionsToStartTrialText, value); } }

        bool _ShowCountdownWindow = default(bool);
        public bool ShowCountdownWindow { get { return _ShowCountdownWindow; } set { base.SetProperty(ref _ShowCountdownWindow, value); } }

        bool _ShowMessageBoxWindow = default(bool);
        public bool ShowMessageBoxWindow { get { return _ShowMessageBoxWindow; } set { base.SetProperty(ref _ShowMessageBoxWindow, value); } }

        string _TextForMessageBoxWindow = default(string);
        public string TextForMessageBoxWindow { get { return _TextForMessageBoxWindow; } set { base.SetProperty(ref _TextForMessageBoxWindow, value); } }

        bool _ShowSkeleton = default(bool);
        public bool ShowSkeleton { get { return _ShowSkeleton; } set { base.SetProperty(ref _ShowSkeleton, value); } }

        bool _ShowFingerTip = default(bool);
        public bool ShowFingerTip { get { return _ShowFingerTip; } set { base.SetProperty(ref _ShowFingerTip, value); } }

        bool _IsKinectTracking = default(bool);
        public bool IsKinectTracking { get { return _IsKinectTracking; } set { base.SetProperty(ref _IsKinectTracking, value); } }

        string _StatusText = default(string);
        public string StatusText { get { return _StatusText; } set { base.SetProperty(ref _StatusText, value); } }

        Point trackCenter = new Point(Settings.Default.TrackLeftX + Settings.Default.TrackRadius, Settings.Default.TrackTopY + Settings.Default.TrackRadius);
        DispatcherTimer gameTimer;
        public Point AbsoluteScreenPositionOfTarget { get; set; }
        private double TargetSizeRadius;


        private bool _isUsingKinect;

        public bool IsUsingKinect
        {
            get { return _isUsingKinect; }
            set { _isUsingKinect = value; }
        }

        int _CountdownCount = default(int);
        public int CountdownCount { get { return _CountdownCount; } set { base.SetProperty(ref _CountdownCount, value); } }


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
                        Results = new Models.Result { TimeOnTarget = 0, AbsoluteError = 0, AbsoluteErrorForEachTickList = new List<double>() }
                    });
                }

                // set the current trial so user sees it on first launch
                this.CurrentTrial = this.Trials.First();

                // setup the UI
                TrackRadius = Settings.Default.TrackRadius;
                this.UpdateTargetSizeAndPlaceInStartingPosition();

                // some designer info
                ParticipantID = "D<Please enter>";
                StatusText = "D: Status";
            }
        }

        // called by the Main Window on Loaded event
        public void Startup()
        {
            // To be eventually replaced by the researcher uploading a file or something
            for (int i = 0; i < 3; i++)
            {
                AddTrial();
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

            // show the UI we want user to see
            this.ShowDebugCommand.Execute(null);
            this.ShowSettingsCommand.Execute(null);

            // placeholder
            ParticipantID = "<Please enter>";
        }

        private void AddTrial()
        {
            int currentCount = Trials.Count;

            this.Trials.Add(new Models.Trial
            {
                Number = currentCount, // Number uses a converter + 1, // no 0-based trials exposed to user
                Results = new Models.Result
                {
                    TimeOnTarget = 0,
                    AbsoluteError = 0,
                    AbsoluteErrorForEachTickList = new List<double>(),
                    IsInsideTrackForEachTickList = new List<bool>()
                }
            });

            // to see real-time updates when user makes modifications in Settings Window
            this.Trials[currentCount].PropertyChanged += CurrentTrial_PropertyChanged;
        }

        private void AddTrial(double diameter, double duration, double rpm)
        {
            int currentCount = Trials.Count;

            this.Trials.Add(new Models.Trial
            {
                ShapeSizeDiameter = diameter,
                DurationSeconds = duration,
                RPMs = rpm,
                Number = currentCount, // Number uses a converter + 1, // no 0-based trials exposed to user                
                Results = new Models.Result
                {
                    TimeOnTarget = 0,
                    AbsoluteError = 0,
                    AbsoluteErrorForEachTickList = new List<double>(),
                    IsInsideTrackForEachTickList = new List<bool>()
                }
            });

            // to see real-time updates when user makes modifications in Settings Window
            this.Trials[currentCount].PropertyChanged += CurrentTrial_PropertyChanged;
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
            if (CurrentTrial == null)
            {
                return;
            }

            TargetSizeRadius = CurrentTrial.ShapeSizeDiameter / 2.0;
            this.PlaceTargetInStartingPosition();
        }

        private void InitializeExperimentVariables()
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

        // default will be that the user finishes the experiment
        private void StopExperiment(bool aborted = false)
        {
            // reset UI
            this.IsExperimentRunning = false;
            ResetTargetValues();
            UpdateTargetSizeAndPlaceInStartingPosition();

            // self-explanatory            
            LogData();

            // handle user
            ShowEndOfExperimentWindow(aborted);
        }

        private void ShowEndOfExperimentWindow(bool aborted)
        {
            if (aborted)
            {
                // researcher stops the experiment before it is finished
                TextForMessageBoxWindow = "#SadTrombone - experiment aborted";
            }
            else
            {
                // user finished the experiment
                TextForMessageBoxWindow = "Yay! Experiments is over. Btw, YOU ROCK!!";
            }

            ShowMessageBoxWindow = true;
        }

        private void LogData()
        {
            DataLogger.LogExperiment(ParticipantID, Trials);
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
//        Stopwatch timeOnTarget = new Stopwatch();
        int ticksOnTarget = 0; // stopwatch is not reliable. Going to count the ticks on target and derive the ToT based on percentage of perfect ToT time
        Stopwatch totalTrialTime = new Stopwatch();
        double secondsToDoOneRotation;

        private void StartNextTrial()
        {
            stopTime = DateTime.Now.AddSeconds(CurrentTrial.DurationSeconds);

//            timeOnTarget.Reset();
            ticksOnTarget = 0;
            totalTrialTime.Reset();

            TargetSizeRadius = this.CurrentTrial.ShapeSizeDiameter / 2.0;

            secondsToDoOneRotation = 1 / (CurrentTrial.RPMs / 60.0);

            // rock and roll
            CurrentTrial.Results.TickCount = 0;
            totalTrialTime.Start();
            gameTimer.Start();
        }

        private void StopCurrentTrial()
        {
            totalTrialTime.Stop();
            gameTimer.Stop();

            //Debug.Print("Trial #" + CurrentTrial.Number.ToString() + " stopped at " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
            //Debug.Print("Trial time took " + totalTrialTime.Elapsed.ToString());

            // save results
            double percentageOnTarget = (double)ticksOnTarget / (double)CurrentTrial.Results.TickCount;

            CurrentTrial.Results.TimeOnTarget = percentageOnTarget * CurrentTrial.DurationSeconds;  //timeOnTarget.ElapsedMilliseconds;
            CurrentTrial.Results.AbsoluteError = Statistics.Mean(CurrentTrial.Results.AbsoluteErrorForEachTickList);
            CurrentTrial.Results.ConstantError = Statistics.ConstantError(CurrentTrial.Results.AbsoluteErrorForEachTickList, CurrentTrial.Results.IsInsideTrackForEachTickList);
            CurrentTrial.Results.VariableError = Statistics.VariableError(CurrentTrial.Results.AbsoluteErrorForEachTickList, CurrentTrial.Results.IsInsideTrackForEachTickList);

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

                WaitForUserToStartNextTrial();
            }
        }

        private void WaitForUserToStartNextTrial()
        {
            if (IsUsingKinect)
            {
                // Show Countdown Window
                ShowCountdownWindowUI();
            }
            else
            {
                // wait for user to click the target
                ShowInstructionsToStartTrial = true;
                ShowInstructionsToStartTrialText = "Click the red ball to start the trial";
                MouseUserReadyForNextTrialCanExecute = true;
            }

        }

        private DispatcherTimer waitForHandInsideTargetTimer;
        private void WaitForUserToPutHandInsideTarget()
        {

            waitForHandInsideTargetTimer = new DispatcherTimer();
            waitForHandInsideTargetTimer.Interval = TimeSpan.FromMilliseconds(Settings.Default.MillisecondDelay);
            waitForHandInsideTargetTimer.Tick += CheckForHandInsideTargetTick;
            waitForHandInsideTargetTimer.Start();
        }

        private void CheckForHandInsideTargetTick(object sender, EventArgs e)
        {
            // check whether inside target
            bool result = Tools.IsInsideCircle(this.InputPosition, this.TargetPositionCenter, TargetSizeRadius);

            if (result)
            {
                // stop listening to this event and start the next trial
                waitForHandInsideTargetTimer.Tick -= CheckForHandInsideTargetTick;

                // hide instructions
                ShowInstructionsToStartTrial = false;

                // start next trial
                StartNextTrial();
            }        
        }

        

        private void MoveMouseToStartingPosition()
        {
            throw new System.NotImplementedException("Don't know how to get the view to call AbsoluteScreenPositionOfTarget. But you can call me Al");

            //double x = AbsoluteScreenPositionOfTarget.X + TargetSizeRadius;
            //double y = AbsoluteScreenPositionOfTarget.Y + TargetSizeRadius;

            //// broken
            //SetPosition((int)x, (int)y);
        }

        #region "Import Export Settings"

        private void ImportSettings()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".csv"; // Defaults.FilePaths.SettingsExtension;
            dialog.Filter = "CSV Files (*.csv)|*.csv";

            var result = dialog.ShowDialog();

            // got the file
            if (result == true)
            {
                // must set CurrentTrials to null; otherwise because it is databound to a DP, it will crash
                CurrentTrial = null;
                Trials.Clear();

                ParseImportFile(dialog);

                CurrentTrial = Trials.First();
                UpdateTargetSizeAndPlaceInStartingPosition();
            }
            else
            {
                Debug.Print("User cancelled the operation perhaps?");
            }
        }

        internal void ParseImportFile(Microsoft.Win32.OpenFileDialog dialog)
        {
            try
            {
                // first line is the trialOrder
                using (StreamReader reader = new StreamReader(dialog.FileName))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');

                        AddTrial(Convert.ToDouble(values[0]),
                                 Convert.ToDouble(values[1]),
                                 Convert.ToDouble(values[2]));
                    }

                }
            }
            catch (System.IO.IOException ex)
            {
                MessageBox.Show("Yo! Close the file in Excel" + ex);
            }
        }

        public void ExportSettings()
        {
            TextForMessageBoxWindow = "Yay! Trials settings succesfully exported.";
            ShowMessageBoxWindow = DataLogger.ExportSettings(Trials);
        }

        #endregion //Import export settings


        #region "Game Code"

        void GameTimer_Tick(object sender, EventArgs e)
        {
            CurrentTrial.Results.TickCount++;
            //            Debug.Print("Tick #" + tickCounter++ + " and time is right now " + DateTime.Now.ToString("hh.mm.ss.ffffff"));

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
            distanceFromCenterOnTick = Statistics.DistanceBetween2Points(this.InputPosition, this.TargetPositionCenter);
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
            IsOnTarget = Tools.IsInsideCircle(this.InputPosition, this.TargetPositionCenter, TargetSizeRadius);

            if (IsOnTarget)
            {
                ticksOnTarget++;
//                timeOnTarget.Start();
            }
            //else
            //{
            //    timeOnTarget.Stop();
            //}
        }

        bool isInsideTrackCircle;
        private void CheckIsInsideTrackCircle()
        {
            // calculate whether inside the track for algebraic error
            isInsideTrackCircle = Tools.IsInsideCircle(this.InputPosition, this.trackCenter, this.TrackRadius);

            // save boolean
            CurrentTrial.Results.IsInsideTrackForEachTickList.Add(isInsideTrackCircle);
        }

        #endregion //Game Code

        #region Commands

        bool _MouseUserReadyForNextTrialCanExecute = default(bool);
        public bool MouseUserReadyForNextTrialCanExecute { get { return _MouseUserReadyForNextTrialCanExecute; } set { base.SetProperty(ref _MouseUserReadyForNextTrialCanExecute, value); } }

        DelegateCommand _UserClickedTargetToStartNextTrialCommand = null;
        public DelegateCommand UserClickedTargetToStartNextTrialCommand
        {
            get
            {
                if (_UserClickedTargetToStartNextTrialCommand != null)
                    return _UserClickedTargetToStartNextTrialCommand;

                _UserClickedTargetToStartNextTrialCommand = new DelegateCommand
                (
                    () =>
                    {
                        ShowInstructionsToStartTrial = false;
                        MouseUserReadyForNextTrialCanExecute = false;

                        StartNextTrial();
                    },
                    () =>
                    {
                        return MouseUserReadyForNextTrialCanExecute;
                    }
                );
                this.PropertyChanged += (s, e) => _UserClickedTargetToStartNextTrialCommand.RaiseCanExecuteChanged();
                return _UserClickedTargetToStartNextTrialCommand;
            }
        }

        DelegateCommand _ShowParticipantInstructionsWindowCommand = null;
        public DelegateCommand ShowParticipantInstructionsWindowCommand
        {
            get
            {
                if (_ShowParticipantInstructionsWindowCommand != null)
                    return _ShowParticipantInstructionsWindowCommand;

                _ShowParticipantInstructionsWindowCommand = new DelegateCommand
                (
                    () =>
                    {
                        ShowInstructionsToStartTrial = false;
                        ShowParticipantInstructions = true;
                    },
                    () =>
                    {
                        // this should only be allowed when the ParticipantWindow is showing
                        return true;
                    }
                );
                this.PropertyChanged += (s, e) => _ShowParticipantInstructionsWindowCommand.RaiseCanExecuteChanged();
                return _ShowParticipantInstructionsWindowCommand;
            }
        }

        // for both kinect mode and mouse mode - the researcher has clicked Start Experiment
        DelegateCommand _StartExperimentCommand = null;
        public DelegateCommand StartExperimentCommand
        {
            get
            {
                if (_StartExperimentCommand != null)
                    return _StartExperimentCommand;

                _StartExperimentCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (IsUsingKinect)
                        {                            
                            ShowCountdownWindowUI();
                        }
                        else
                        {
                            ShowParticipantInstructions = false;
                            ShowInstructionsToStartTrial = true;
                            MouseUserReadyForNextTrialCanExecute = true;
                            ShowInstructionsToStartTrialText = "Click the red ball to start the trial";
                        }

                    },
                    () =>
                    {
                        return true; // what to do here????
                    }
                );
                this.PropertyChanged += (s, e) => _StartExperimentCommand.RaiseCanExecuteChanged();
                return _StartExperimentCommand;
            }
        }

        // create into its own class???
        const int CountdownInSeconds = 3;
        private DispatcherTimer countdownWindowTimer;

        private void ShowCountdownWindowUI()
        {
            ShowParticipantInstructions = false;
            ShowCountdownWindow = true;
            CountdownCount = CountdownInSeconds;

            countdownWindowTimer = new DispatcherTimer();
            countdownWindowTimer.Interval = TimeSpan.FromSeconds(1);
            countdownWindowTimer.Tick += CountdownWindowTimerTick;
            countdownWindowTimer.Start();
        }

        private void CountdownWindowTimerTick(object sender, EventArgs e)
        {
            CountdownCount--;

            // hide when we hit 0
            if (CountdownCount <= 0)
            {
                countdownWindowTimer.Stop();
                countdownWindowTimer.Tick -= CountdownWindowTimerTick;
                
                // update UI
                ShowCountdownWindow = false;

                // show instructions to user to put their hand in target and wait for the event
                ShowInstructionsToStartTrial = true;
                ShowInstructionsToStartTrialText = "Put your hand in the target to start next trial";

                // wait for user to put their hand inside the target
                WaitForUserToPutHandInsideTarget();
            }
        }




        bool _IsSettingsWindowOpen = default(bool);
        public bool IsSettingsWindowOpen { get { return _IsSettingsWindowOpen; } set { base.SetProperty(ref _IsSettingsWindowOpen, value); } }

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

                        var main = Application.Current.MainWindow;

                        settings.Left = main.Left + 45;
                        settings.Top = main.Top + 60;

                        settings.Owner = main; // whatever happens to main window happens here                                               
                        settings.ShowInTaskbar = false;
                        IsSettingsWindowOpen = true;
                        settings.Show();
                    },
                    () =>
                    {
                        return (!IsSettingsWindowOpen) && (!IsExperimentRunning);
                    }
                );
                this.PropertyChanged += (s, e) => _ShowSettingsCommand.RaiseCanExecuteChanged();
                return _ShowSettingsCommand;
            }
        }

        private void SettingsWindowSetupCallback()
        {
            IsSettingsWindowOpen = false;

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
            return IsSettingsWindowOpen;
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
            return IsSettingsWindowOpen;
        }

        DelegateCommand _StartExpCommand = null;
        public DelegateCommand StartExpCommand
        {
            get
            {
                if (_StartExpCommand != null)
                    return _StartExpCommand;

                _StartExpCommand = new DelegateCommand(new Action(StartResetExperiment), new Func<bool>(StartExpCanExecute));
                this.PropertyChanged += (s, e) => _StartExpCommand.RaiseCanExecuteChanged();
                return _StartExpCommand;
            }
        }
        public bool StartExpCanExecute()
        {
            return true; //should be changed to only start when ParticipantInstructions Window is showing
        }

        public void StartResetExperiment()
        {
            if (!IsExperimentRunning)
            {
                StartExperiment();
            }
            else
            {
                AbortExperiment();
            }
        }

        private void AbortExperiment()
        {
            StopCurrentTrial();
            StopExperiment(aborted: true);
            ShowInstructionsToStartTrial = false;
        }


        private void RestartExperiment()
        {
            StopCurrentTrial();
            StopExperiment();
            StartExperiment();
        }

        private void StartExperiment()
        {
            InitializeExperimentVariables();
            ShowInstructionsToStartTrial = false;
            ShowParticipantInstructions = true;
            //StartNextTrial();
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

        DelegateCommand _AddTrialCommand = null;
        public DelegateCommand AddTrialCommand
        {
            get
            {
                if (_AddTrialCommand != null)
                    return _AddTrialCommand;

                _AddTrialCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        AddTrial();   // can only add a trial if the settings window is opened
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSettingsWindowOpen;
                        }));
                this.PropertyChanged += (s, e) => _AddTrialCommand.RaiseCanExecuteChanged();
                return _AddTrialCommand;
            }
        }

        DelegateCommand _ImportSettingsCommand = null;
        public DelegateCommand ImportSettingsCommand
        {
            get
            {
                if (_ImportSettingsCommand != null)
                    return _ImportSettingsCommand;

                _ImportSettingsCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        ImportSettings();
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSettingsWindowOpen;
                        }));
                this.PropertyChanged += (s, e) => _ImportSettingsCommand.RaiseCanExecuteChanged();
                return _ImportSettingsCommand;
            }
        }

        DelegateCommand _ExportSettingsCommand = null;
        public DelegateCommand ExportSettingsCommand
        {
            get
            {
                if (_ExportSettingsCommand != null)
                    return _ExportSettingsCommand;

                _ExportSettingsCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        ExportSettings();
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSettingsWindowOpen;
                        }));
                this.PropertyChanged += (s, e) => _ExportSettingsCommand.RaiseCanExecuteChanged();
                return _ExportSettingsCommand;
            }
        }

        DelegateCommand _CloseMessageBoxWindowCommand = null;
        public DelegateCommand CloseMessageBoxWindowCommand
        {
            get
            {
                if (_CloseMessageBoxWindowCommand != null)
                    return _CloseMessageBoxWindowCommand;

                _CloseMessageBoxWindowCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        ShowMessageBoxWindow = false;
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return ShowMessageBoxWindow; // only if this window is opened
                        }));
                this.PropertyChanged += (s, e) => _CloseMessageBoxWindowCommand.RaiseCanExecuteChanged();
                return _CloseMessageBoxWindowCommand;
            }
        }

        DelegateCommand _OpenResultsFolderCommand = null;
        public DelegateCommand OpenResultsFolderCommand
        {
            get
            {
                if (_OpenResultsFolderCommand != null)
                    return _OpenResultsFolderCommand;

                _OpenResultsFolderCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        string dir = DataLogger.GetDesktopFolder();
                        Process.Start(dir);
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return !IsExperimentRunning; // only if the experiment is not running
                        }));
                this.PropertyChanged += (s, e) => _OpenResultsFolderCommand.RaiseCanExecuteChanged();
                return _OpenResultsFolderCommand;
            }
        }

        #endregion


    }
}
