using Microsoft.Kinect;
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
    public enum Side { Right, Left }

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

        string _ShowParticipantInstructionsText = default(string);
        public string ShowParticipantInstructionsText { get { return _ShowParticipantInstructionsText; } set { base.SetProperty(ref _ShowParticipantInstructionsText, value); } }

        bool _ShowCountdownWindow = default(bool);
        public bool ShowCountdownWindow { get { return _ShowCountdownWindow; } set { base.SetProperty(ref _ShowCountdownWindow, value); } }

        bool _ShowMessageBoxWindow = default(bool);
        public bool ShowMessageBoxWindow { get { return _ShowMessageBoxWindow; } set { base.SetProperty(ref _ShowMessageBoxWindow, value); } }

        string _TextForMessageBoxWindow = default(string);
        public string TextForMessageBoxWindow { get { return _TextForMessageBoxWindow; } set { base.SetProperty(ref _TextForMessageBoxWindow, value); } }

        bool _ShowSkeleton = default(bool);
        public bool ShowSkeleton { get { return _ShowSkeleton; } set { base.SetProperty(ref _ShowSkeleton, value); } }

        bool _ShowTrack = default(bool);
        public bool ShowTrack { get { return _ShowTrack; } set { base.SetProperty(ref _ShowTrack, value); } }

        int _CountdownTimeInSeconds = default(int);
        public int CountdownTimeInSeconds { get { return _CountdownTimeInSeconds; } set { base.SetProperty(ref _CountdownTimeInSeconds, value); } }

        bool _ShowFingerTip = default(bool);
        public bool ShowFingerTip { get { return _ShowFingerTip; } set { base.SetProperty(ref _ShowFingerTip, value); } }

        bool _IsKinectTracking = default(bool);
        public bool IsKinectTracking { get { return _IsKinectTracking; } set { base.SetProperty(ref _IsKinectTracking, value); } }

        string _StatusText = default(string);
        public string StatusText { get { return _StatusText; } set { base.SetProperty(ref _StatusText, value); } }

        Point TrackCenter = new Point(Settings.Default.TrackLeftX + Settings.Default.TrackRadius, Settings.Default.TrackTopY + Settings.Default.TrackRadius);
        DispatcherTimer gameTimer;
        public Point AbsoluteScreenPositionOfTarget { get; set; }
        private double TargetSizeRadius;
        private bool _isUsingKinect;

        public bool IsUsingKinect { get { return _isUsingKinect; } set { _isUsingKinect = value; } }

        int _CountdownCount = default(int);
        public int CountdownCount { get { return _CountdownCount; } set { base.SetProperty(ref _CountdownCount, value); } }

        private double _TickHandDepth;
        public double TickHandDepth { get { return _TickHandDepth; } set { _TickHandDepth = value; } }

        private PointF _TickLeanAmount;
        public PointF TickLeanAmount { get { return _TickLeanAmount; } set { _TickLeanAmount = value; } }

        private Point _Elbow;
        public Point Elbow { get { return _Elbow; } set { _Elbow = value; } }

        private Side _Handedness;
        public Side Handedness { get { return _Handedness; } set { base.SetProperty(ref _Handedness, value); } }

        private bool _AlignHips;
        public bool AlignHips { get { return _AlignHips; } set { base.SetProperty(ref _AlignHips, value); } }

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
                        Results = new Models.Result
                        {
                            TimeOnTarget = 0,
                            AbsoluteError = 0,
                            AbsoluteErrorForEachTickList = new List<double>(),
                        }
                    });
                }

                // set the current trial so user sees it on first launch
                this.CurrentTrial = this.Trials.First();

                // setup the UI
                TrackRadius = Settings.Default.TrackRadius;
                this.UpdateTargetSizeAndPlaceInStartingPosition();

                // some designer info                
                StatusText = "D: Status";

                // show track in designer
                ShowTrack = true;
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

            // other defaults to use
            Handedness = Side.Right;
            ShowTrack = true;
            CountdownTimeInSeconds = 3;
        }

        public void ShowSettingsOnLaunch()
        {
            // show the UI we want user to see
            this.ShowSettingsCommand.Execute(null);
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
                    IsInsideTrackForEachTickList = new List<bool>(),
                    HandDepthForEachTickList = new List<double>(),
                    LeanAmountForEachTickList = new List<PointF>(),
                    OnTargetList = new List<bool>()
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
                    IsInsideTrackForEachTickList = new List<bool>(),
                    HandDepthForEachTickList = new List<double>(),
                    LeanAmountForEachTickList = new List<PointF>(),
                    OnTargetList = new List<bool>()
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

            ClearAllResults();
        }

        private void ClearAllResults()
        {
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
                TextForMessageBoxWindow =
                    "Experiment aborted.\n" +
                    "\n" +
                    "All data collected (including in-progress trials) have been recorded and saved.";
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

            CalculateTrialResults();
        }

        private void CalculateTrialResults()
        {

            CalculateTotalTrialResults();

            // break up the results into thirds
            List<double> ABE1, ABE2, ABE3;
            Tools.BreakListIntoThirds(CurrentTrial.Results.AbsoluteErrorForEachTickList, out ABE1, out ABE2, out ABE3);

            List<bool> IsInside1, IsInside2, IsInside3;
            Tools.BreakListIntoThirds(CurrentTrial.Results.IsInsideTrackForEachTickList, out IsInside1, out IsInside2, out IsInside3);

            List<bool> OnTarget1, OnTarget2, OnTarget3;
            Tools.BreakListIntoThirds(CurrentTrial.Results.OnTargetList, out OnTarget1, out OnTarget2, out OnTarget3);

            // get 1/3 time percentages
            double firstTimePercentage, secondTimePercentage, thirdTimePercentage;
            Tools.GetTimeThirdPercentages(CurrentTrial.Results.TickCount, out firstTimePercentage, out secondTimePercentage, out thirdTimePercentage);

            // first 1/3rd            
            CalculateFirstThirdResults(ABE1, IsInside1, OnTarget1, firstTimePercentage);

            // second 1/3rd            
            CalculateSecondThirdResults(ABE2, IsInside2, OnTarget2, secondTimePercentage);

            // third 1/3rd
            CalculateThirdThirdResults(ABE3, IsInside3, OnTarget3, thirdTimePercentage);

            if (IsUsingKinect)
            {
                CalculateKinectThirdResults();
            }
        }

        private void CalculateTotalTrialResults()
        {
            double percentageOnTarget = (double)ticksOnTarget / (double)CurrentTrial.Results.TickCount;
            CurrentTrial.Results.TimeOnTarget = Math.Round(percentageOnTarget * CurrentTrial.DurationSeconds, 3);

            CurrentTrial.Results.AbsoluteError = Statistics.Mean(CurrentTrial.Results.AbsoluteErrorForEachTickList);
            CurrentTrial.Results.ConstantError = Statistics.ConstantError(CurrentTrial.Results.AbsoluteErrorForEachTickList, CurrentTrial.Results.IsInsideTrackForEachTickList);
            CurrentTrial.Results.VariableError = Statistics.VariableError(CurrentTrial.Results.AbsoluteErrorForEachTickList, CurrentTrial.Results.IsInsideTrackForEachTickList);
            CurrentTrial.Results.HandDepthStdDev = Statistics.PopulationStandardDeviation(CurrentTrial.Results.HandDepthForEachTickList);
            CurrentTrial.Results.LeanLeftRightX = Statistics.PopulationStandardDeviation(CurrentTrial.Results.LeanAmountForEachTickList, DesiredCoord.X);
            CurrentTrial.Results.LeanForwardBackY = Statistics.PopulationStandardDeviation(CurrentTrial.Results.LeanAmountForEachTickList, DesiredCoord.Y);
        }

        private void CalculateFirstThirdResults(List<double> ABE1, List<bool> IsInside1, List<bool> OnTarget1, double firstTimePercentage)
        {
            CurrentTrial.Results.TimeOnTarget1 = Tools.CalculateTimeThirds(OnTarget1, CurrentTrial.DurationSeconds, firstTimePercentage);

            CurrentTrial.Results.AbsoluteError1 = Statistics.Mean(ABE1);
            CurrentTrial.Results.ConstantError1 = Statistics.ConstantError(ABE1, IsInside1);
            CurrentTrial.Results.VariableError1 = Statistics.VariableError(ABE1, IsInside1);
            CurrentTrial.Results.TickCount1 = ABE1.Count; // size of list, not the sum
        }

        private void CalculateSecondThirdResults(List<double> ABE2, List<bool> IsInside2, List<bool> OnTarget2, double secondTimePercentage)
        {
            CurrentTrial.Results.TimeOnTarget2 = Tools.CalculateTimeThirds(OnTarget2, CurrentTrial.DurationSeconds, secondTimePercentage);

            CurrentTrial.Results.AbsoluteError2 = Statistics.Mean(ABE2);
            CurrentTrial.Results.ConstantError2 = Statistics.ConstantError(ABE2, IsInside2);
            CurrentTrial.Results.VariableError2 = Statistics.VariableError(ABE2, IsInside2);
            CurrentTrial.Results.TickCount2 = ABE2.Count;
        }

        private void CalculateThirdThirdResults(List<double> ABE3, List<bool> IsInside3, List<bool> OnTarget3, double thirdTimePercentage)
        {
            CurrentTrial.Results.TimeOnTarget3 = Tools.CalculateTimeThirds(OnTarget3, CurrentTrial.DurationSeconds, thirdTimePercentage);

            CurrentTrial.Results.AbsoluteError3 = Statistics.Mean(ABE3);
            CurrentTrial.Results.ConstantError3 = Statistics.ConstantError(ABE3, IsInside3);
            CurrentTrial.Results.VariableError3 = Statistics.VariableError(ABE3, IsInside3);
            CurrentTrial.Results.TickCount3 = ABE3.Count;
        }


        private void CalculateKinectThirdResults()
        {
            List<double> HandDepth1, HandDepth2, HandDepth3;
            Tools.BreakListIntoThirds(CurrentTrial.Results.HandDepthForEachTickList, out HandDepth1, out HandDepth2, out HandDepth3);

            List<PointF> LeanAmount1, LeanAmount2, LeanAmount3;
            Tools.BreakListIntoThirds(CurrentTrial.Results.LeanAmountForEachTickList, out LeanAmount1, out LeanAmount2, out LeanAmount3);

            // first 1/3rd
            CurrentTrial.Results.HandDepthStdDev1 = Statistics.PopulationStandardDeviation(HandDepth1);
            CurrentTrial.Results.LeanLeftRightX1 = Statistics.PopulationStandardDeviation(LeanAmount1, DesiredCoord.X);
            CurrentTrial.Results.LeanForwardBackY1 = Statistics.PopulationStandardDeviation(LeanAmount1, DesiredCoord.Y);

            // second 1/3rd
            CurrentTrial.Results.HandDepthStdDev2 = Statistics.PopulationStandardDeviation(HandDepth2);
            CurrentTrial.Results.LeanLeftRightX2 = Statistics.PopulationStandardDeviation(LeanAmount2, DesiredCoord.X);
            CurrentTrial.Results.LeanForwardBackY2 = Statistics.PopulationStandardDeviation(LeanAmount2, DesiredCoord.Y);

            // third 1/3rd
            CurrentTrial.Results.HandDepthStdDev3 = Statistics.PopulationStandardDeviation(HandDepth3);
            CurrentTrial.Results.LeanLeftRightX3 = Statistics.PopulationStandardDeviation(LeanAmount3, DesiredCoord.X);
            CurrentTrial.Results.LeanForwardBackY3 = Statistics.PopulationStandardDeviation(LeanAmount3, DesiredCoord.Y);
        }

        private void SetupNextTrial()
        {
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

                // wait for user to put their hand inside the ball
                WaitForUserToStartNextTrial();
            }
        }

        private void WaitForUserToStartNextTrial()
        {
            // In kinect mode
            if (IsUsingKinect)
            {
                // show if not the first trial
                ShowCountdownWindowUI();
            }
            else
            {
                // in mouse mode, wait for user to click the target
                WaitForUserToClickTarget();
            }

        }

        private void WaitForUserToClickTarget()
        {
            ShowInstructionsToStartTrial = true;
            ShowInstructionsToStartTrialText = "Click the yellow ball to start the trial";
            MouseUserReadyForNextTrialCanExecute = true;
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
            // looking for the inner 1/3 of the circle to start
            bool result = Tools.IsInsideCircle(this.InputPosition, this.TargetPositionCenter, TargetSizeRadius / 3.0);

            if (result)
            {
                // stop listening to this event and start the next trial
                waitForHandInsideTargetTimer.Tick -= CheckForHandInsideTargetTick;
                waitForHandInsideTargetTimer.Stop();

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

            if (IsUsingKinect)
            {
                CurrentTrial.Results.LeanAmountForEachTickList.Add(TickLeanAmount);
                CurrentTrial.Results.HandDepthForEachTickList.Add(TickHandDepth);
            }

            if (HasTrialTimeExpired())
            {
                StopCurrentTrial();
                SetupNextTrial();
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
            distanceFromCenterOnTick = Tools.DistanceBetween2Points(this.InputPosition, this.TargetPositionCenter);
            CurrentTrial.Results.AbsoluteErrorForEachTickList.Add(distanceFromCenterOnTick);
        }

        double xt, yt;
        private void MoveTargetOnTick()
        {
            Common.Tools.PointsOnACircle(TrackRadius, AngleInDegrees, TrackCenter, out xt, out yt);
            TargetPositionLeft = xt - TargetSizeRadius;
            TargetPositionTop = yt - TargetSizeRadius;
        }

        private bool HasTrialTimeExpired()
        {
            return (DateTime.Now > stopTime);
        }

        private void PlaceTargetInStartingPosition()
        {
            Point pt = Tools.GetPointForPlacingTargetInStartingPosition(TrackCenter, TrackRadius, TargetSizeRadius);

            TargetPositionLeft = pt.X;
            TargetPositionTop = pt.Y;
        }

        private void CheckIsOnTarget()
        {
            // calculate whether inside the circle
            IsOnTarget = Tools.IsInsideCircle(this.InputPosition, this.TargetPositionCenter, TargetSizeRadius);

            CurrentTrial.Results.OnTargetList.Add(IsOnTarget);

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
            isInsideTrackCircle = Tools.IsInsideCircle(this.InputPosition, this.TrackCenter, this.TrackRadius);

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
                        ShowParticipantInstructionsUI();
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

        private void ShowParticipantInstructionsUI()
        {
            ShowParticipantInstructions = true;

            string experimentRunTimeInMinutes = CalculateExperimentRunTimeInMinutes();

            if (IsUsingKinect)
            {
                ShowParticipantInstructionsText =
                    "In this experiment, you will track a red circle moving\n" +
                    "steadily around a circular path. Try to keep the tips of\n" +
                    "your fingers of your dominant hand within the red circle\n" +
                    "at all times.\n" +
                    "\n" +
                    "Please keep your arm out as far away from your body as comfortable.\n" +
                    "\n" +
                    "There will be a total of " + Trials.Count + " trials. Between each trial,\n" +
                    "you will have a 10 second break where you will rest your arm\n" +
                    "at your side.\n" +
                    "\n" +
                    "The experiment will last approximately " + experimentRunTimeInMinutes + "\n" +
                    "\n" +
                    "Say \"Ready\" to being the experiment.";
            }
            else
            {
                ShowParticipantInstructionsText =
                    "In this experiment, you will track a red circle moving\n" +
                    "steadily around a circular path. Try to keep your mouse\n" +
                    "pointer within the red circle at all times.\n" +
                    "\n" +
                    "There will be a total of " + Trials.Count + " trials\n" +
                    "\n" +
                    "The experiment will last approximately " + experimentRunTimeInMinutes + "\n" +
                    "\n" +
                    "Say \"Ready\" to being the experiment.";
            }

        }

        private string CalculateExperimentRunTimeInMinutes()
        {
            double time = 0;
            double breakTime = 0;

            if (IsUsingKinect)
            {
                breakTime = CountdownTimeInSeconds;
            }

            foreach (var trial in Trials)
            {
                time += trial.DurationSeconds + breakTime;
            }

            // convert into minutes
            int seconds = (int)time % 60;
            int minutes = (int)time / 60;

            if (seconds == 0)
            {
                return minutes + " minutes ";
            }
            else
            {
                return minutes + " minutes and " + seconds + " seconds";
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
                        // hide instructions window
                        ShowParticipantInstructions = false;

                        if (IsUsingKinect)
                        {
                            // don't show countdown window. it is just confusing.
                            if (this.CurrentTrial.Number == 0)
                            {
                                ShowInstructionsToPutHandInTargetAndWait();
                            }
                            else
                            {
                                ShowCountdownWindowUI();
                            }
                        }
                        else
                        {
                            // mouse mode
                            WaitForUserToClickTarget();
                        }

                    },
                    () =>
                    {
                        return true; // can always either start or abort
                    }
                );
                this.PropertyChanged += (s, e) => _StartExperimentCommand.RaiseCanExecuteChanged();
                return _StartExperimentCommand;
            }
        }

        // probably should have created it into its own class
        private DispatcherTimer countdownWindowTimer;

        private void ShowCountdownWindowUI()
        {
            ShowCountdownWindow = true;
            CountdownCount = CountdownTimeInSeconds;

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
                ShowInstructionsToPutHandInTargetAndWait();
            }
        }

        private void ShowInstructionsToPutHandInTargetAndWait()
        {
            ShowInstructionsToStartTrial = true;
            ShowInstructionsToStartTrialText = "Put your hand in the target to start next trial";

            // wait for user to put their hand inside the target
            WaitForUserToPutHandInsideTarget();
        }

        private void AbortCountdownTimer()
        {
            if (countdownWindowTimer == null)
            {
                return;
            }

            if (!countdownWindowTimer.IsEnabled)
            {
                return;
            }

            countdownWindowTimer.Stop();
            countdownWindowTimer.Tick -= CountdownWindowTimerTick;
            ShowCountdownWindow = false;
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

        DelegateCommand _CloseSettingsWindowCommand = null;
        public DelegateCommand CloseSettingsWindowCommand
        {
            get
            {
                if (_CloseSettingsWindowCommand != null)
                    return _CloseSettingsWindowCommand;

                _CloseSettingsWindowCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        var settingsWin = Application.Current.Windows.OfType<Views.SettingsWindow>().First();
                        settingsWin.Close();
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSettingsWindowOpen; // only if this window is opened
                        }));
                this.PropertyChanged += (s, e) => _CloseSettingsWindowCommand.RaiseCanExecuteChanged();
                return _CloseSettingsWindowCommand;
            }
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
            // wanted to abort experiment but can't figure out what is going on with Countdown Window always executing
            //return !IsExperimentRunning;
            return true;
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

            // if user is about to put their hands in the trial.
            if (this.IsUsingKinect && ShowInstructionsToStartTrial)
            {
                waitForHandInsideTargetTimer.Tick -= CheckForHandInsideTargetTick;
                waitForHandInsideTargetTimer.Stop();                 
            }

            AbortCountdownTimer(); // function checks whether countdown window is enabled

            StopExperiment(aborted: true);
            ShowParticipantInstructions = false;
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
            ShowParticipantInstructionsUI();
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

        DelegateCommand _CenterTargetOnParticipantElbow = null;
        public DelegateCommand CenterTargetOnParticipantElbow
        {
            get
            {
                if (_CenterTargetOnParticipantElbow != null)
                    return _CenterTargetOnParticipantElbow;

                _CenterTargetOnParticipantElbow = new DelegateCommand(new Action(
                    () =>
                    {
                        CalculatePursuitTrackRadius();

                        Settings.Default.TrackLeftX = this.Elbow.X - TrackRadius;
                        Settings.Default.TrackTopY = this.Elbow.Y - TrackRadius;

                        TrackCenter.X = this.Elbow.X;
                        TrackCenter.Y = this.Elbow.Y;

                        this.PlaceTargetInStartingPosition();

                    }),

                    new Func<bool>(
                        () =>
                        {
                            return !IsExperimentRunning; // only if experiment is not running
                        }));
                this.PropertyChanged += (s, e) => _CenterTargetOnParticipantElbow.RaiseCanExecuteChanged();
                return _CenterTargetOnParticipantElbow;
            }
        }

        private void CalculatePursuitTrackRadius()
        {
            double distance = Tools.DistanceBetween2Points(this.Elbow, this.InputPosition);
            TrackRadius = distance;
        }


        DelegateCommand _ClearResultsCommand = null;
        public DelegateCommand ClearResultsCommand
        {
            get
            {
                if (_ClearResultsCommand != null)
                    return _ClearResultsCommand;

                _ClearResultsCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        ClearAllResults();
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSettingsWindowOpen; // only if settings window is showing
                        }));
                this.PropertyChanged += (s, e) => _ClearResultsCommand.RaiseCanExecuteChanged();
                return _ClearResultsCommand;
            }
        }

        DelegateCommand _SetLeftHandedness = null;
        public DelegateCommand SetLeftHandedness
        {
            get
            {
                if (_SetLeftHandedness != null)
                    return _SetLeftHandedness;

                _SetLeftHandedness = new DelegateCommand(new Action(
                    () =>
                    {
                        Handedness = Side.Left;
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return !IsExperimentRunning; // only if settings window is showing
                        }));
                this.PropertyChanged += (s, e) => _SetLeftHandedness.RaiseCanExecuteChanged();
                return _SetLeftHandedness;
            }
        }

        DelegateCommand _SetRightHandedness = null;
        public DelegateCommand SetRightHandedness
        {
            get
            {
                if (_SetRightHandedness != null)
                    return _SetRightHandedness;

                _SetRightHandedness = new DelegateCommand(new Action(
                    () =>
                    {
                        Handedness = Side.Right;
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return !IsExperimentRunning; // only if settings window is showing
                        }));
                this.PropertyChanged += (s, e) => _SetRightHandedness.RaiseCanExecuteChanged();
                return _SetRightHandedness;
            }
        }

        private bool IsSetupWindowOpen = false;
        DelegateCommand _ShowSetupCommand = null;
        public DelegateCommand ShowSetupCommand
        {
            get
            {
                if (_ShowSetupCommand != null)
                    return _ShowSetupCommand;

                _ShowSetupCommand = new DelegateCommand
                (
                    () =>
                    {
                        var setup = new Views.SetupWindow(SetupWindowSetupCallback);
                        setup.DataContext = this; // to share same model data

                        var main = Application.Current.MainWindow;

                        setup.Left = main.Left + 45;
                        setup.Top = main.Top + 60;

                        setup.Owner = main; // whatever happens to main window happens here                                               
                        setup.ShowInTaskbar = false;
                        IsSetupWindowOpen = true;
                        setup.Show();
                    },
                    () =>
                    {
                        return (!IsSetupWindowOpen) && (!IsExperimentRunning) && (IsKinectTracking);
                    }
                );
                this.PropertyChanged += (s, e) => _ShowSetupCommand.RaiseCanExecuteChanged();
                return _ShowSetupCommand;
            }
        }

        private void SetupWindowSetupCallback()
        {
            IsSetupWindowOpen = false;
        }

        DelegateCommand _HideShowHipMarkersCommand = null;
        public DelegateCommand HideShowHipMarkersCommand
        {
            get
            {
                if (_HideShowHipMarkersCommand != null)
                    return _HideShowHipMarkersCommand;

                _HideShowHipMarkersCommand = new DelegateCommand(new Action(
                    () =>
                    {
                        AlignHips = !AlignHips;
                    }),

                    new Func<bool>(
                        () =>
                        {
                            return IsSetupWindowOpen; // only if setup window is showing
                        }));
                this.PropertyChanged += (s, e) => _HideShowHipMarkersCommand.RaiseCanExecuteChanged();
                return _HideShowHipMarkersCommand;
            }
        }


        #endregion
    }
}
