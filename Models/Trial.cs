/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Models
{
    public class Trial : Common.BindableBase
    {

        double _shapeSizeDiameter = default(double);
        public double ShapeSizeDiameter { get { return _shapeSizeDiameter; } set { base.SetProperty(ref _shapeSizeDiameter, value); } }

        double _durationSeconds = default(double);
        public double DurationSeconds { get { return _durationSeconds; } set { base.SetProperty(ref _durationSeconds, value); } }

        double _rpm = default(double);
        public double RPMs { get { return _rpm; } set { base.SetProperty(ref _rpm, value); } }

        int _number = default(int);
        public int Number { get { return _number; } set { base.SetProperty(ref _number, value); } }

        int _trialName = default(int);
        public int TrialName { get { return (Number + 1); } set { base.SetProperty(ref _trialName, value); } }

        Models.Result _Results = default(Models.Result);
        public Models.Result Results { get { return _Results; } set { _Results = value; } }

        public Trial()
        {
            _shapeSizeDiameter = 50;
            _durationSeconds = 5;
            _rpm = 10; 
        }

        internal void ClearResults()
        {
            Results.AbsoluteErrorForEachTickList.Clear();
            Results.IsInsideTrackForEachTickList.Clear();
            Results.HandDepthForEachTickList.Clear();
            Results.LeanAmountForEachTickList.Clear();
            Results.KinectFPSForEachTickList.Clear();
            Results.OnTargetList.Clear();

            Results.AbsoluteError = 0;
            Results.AbsoluteError1 = 0;
            Results.AbsoluteError2 = 0;
            Results.AbsoluteError3 = 0;

            Results.ConstantError = 0;
            Results.ConstantError1 = 0;
            Results.ConstantError2 = 0;
            Results.ConstantError3 = 0;

            Results.TimeOnTarget = 0;
            Results.TimeOnTarget1 = 0;
            Results.TimeOnTarget2 = 0;
            Results.TimeOnTarget3 = 0;

            Results.VariableError = 0;
            Results.VariableError1 = 0;
            Results.VariableError2 = 0;
            Results.VariableError3 = 0;

            Results.TotalPossibleTicks = 0;
            Results.TickCount = 0;
            Results.TickCount1 = 0;
            Results.TickCount2 = 0;
            Results.TickCount3 = 0;

            Results.LeanForwardBackY = 0;
            Results.LeanLeftRightX = 0;
            Results.LeanForwardBackY1 = 0;
            Results.LeanLeftRightX1 = 0;
            Results.LeanForwardBackY2 = 0;
            Results.LeanLeftRightX2 = 0;
            Results.LeanForwardBackY3 = 0;
            Results.LeanLeftRightX3 = 0;

            Results.HandDepthStdDev = 0;
            Results.HandDepthStdDev1 = 0;
            Results.HandDepthStdDev2 = 0;
            Results.HandDepthStdDev3 = 0;

            Results.KinectBodyFramesTrial = 0;
        }
    }
}
