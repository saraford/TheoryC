/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Models
{
    public class Result : Common.BindableBase
    {
        double _TimeOnTarget = default(double);
        public double TimeOnTarget { get { return _TimeOnTarget; } set { base.SetProperty(ref _TimeOnTarget, value); } }

        double _TimeOnTarget1 = default(double);
        public double TimeOnTarget1 { get { return _TimeOnTarget1; } set { base.SetProperty(ref _TimeOnTarget1, value); } }

        double _TimeOnTarget2 = default(double);
        public double TimeOnTarget2 { get { return _TimeOnTarget2; } set { base.SetProperty(ref _TimeOnTarget2, value); } }

        double _TimeOnTarget3 = default(double);
        public double TimeOnTarget3 { get { return _TimeOnTarget3; } set { base.SetProperty(ref _TimeOnTarget3, value); } }

        double _AbsoluteError = default(double);
        public double AbsoluteError { get { return _AbsoluteError; } set { base.SetProperty(ref _AbsoluteError, value); } }

        double _AbsoluteError1 = default(double);
        public double AbsoluteError1 { get { return _AbsoluteError1; } set { base.SetProperty(ref _AbsoluteError1, value); } }

        double _AbsoluteError2 = default(double);
        public double AbsoluteError2 { get { return _AbsoluteError2; } set { base.SetProperty(ref _AbsoluteError2, value); } }

        double _AbsoluteError3 = default(double);
        public double AbsoluteError3 { get { return _AbsoluteError3; } set { base.SetProperty(ref _AbsoluteError3, value); } }

        double _VariableError = default(double);
        public double VariableError { get { return _VariableError; } set { base.SetProperty(ref _VariableError, value); } }

        double _VariableError1 = default(double);
        public double VariableError1 { get { return _VariableError1; } set { base.SetProperty(ref _VariableError1, value); } }

        double _VariableError2 = default(double);
        public double VariableError2 { get { return _VariableError2; } set { base.SetProperty(ref _VariableError2, value); } }

        double _VariableError3 = default(double);
        public double VariableError3 { get { return _VariableError3; } set { base.SetProperty(ref _VariableError3, value); } }

        double _ConstantError = default(double);
        public double ConstantError { get { return _ConstantError; } set { base.SetProperty(ref _ConstantError, value); } }

        double _ConstantError1 = default(double);
        public double ConstantError1 { get { return _ConstantError1; } set { base.SetProperty(ref _ConstantError1, value); } }

        double _ConstantError2 = default(double);
        public double ConstantError2 { get { return _ConstantError2; } set { base.SetProperty(ref _ConstantError2, value); } }

        double _ConstantError3 = default(double);
        public double ConstantError3 { get { return _ConstantError3; } set { base.SetProperty(ref _ConstantError3, value); } }

        int _TickCount = default(int);
        public int TickCount { get { return _TickCount; } set { base.SetProperty(ref _TickCount, value); } }

        int _TickCount1 = default(int);
        public int TickCount1 { get { return _TickCount1; } set { base.SetProperty(ref _TickCount1, value); } }

        int _TickCount2 = default(int);
        public int TickCount2 { get { return _TickCount2; } set { base.SetProperty(ref _TickCount2, value); } }
        
        int _TickCount3 = default(int);
        public int TickCount3 { get { return _TickCount3; } set { base.SetProperty(ref _TickCount3, value); } }

        double _HandDepthStdDev = default(double);
        public double HandDepthStdDev { get { return _HandDepthStdDev; } set { base.SetProperty(ref _HandDepthStdDev, value); } }

        double _LeanForwardBackY = default(double);
        public double LeanForwardBackY { get { return _LeanForwardBackY; } set { base.SetProperty(ref _LeanForwardBackY, value); } }

        double _LeanLeftRightX = default(double);
        public double LeanLeftRightX { get { return _LeanLeftRightX; } set { base.SetProperty(ref _LeanLeftRightX, value); } }

        double _HandDepthStdDev1 = default(double);
        public double HandDepthStdDev1 { get { return _HandDepthStdDev1; } set { base.SetProperty(ref _HandDepthStdDev1, value); } }

        double _LeanForwardBackY1 = default(double);
        public double LeanForwardBackY1 { get { return _LeanForwardBackY1; } set { base.SetProperty(ref _LeanForwardBackY1, value); } }

        double _LeanLeftRightX1 = default(double);
        public double LeanLeftRightX1 { get { return _LeanLeftRightX1; } set { base.SetProperty(ref _LeanLeftRightX1, value); } }

        double _HandDepthStdDev2 = default(double);
        public double HandDepthStdDev2 { get { return _HandDepthStdDev2; } set { base.SetProperty(ref _HandDepthStdDev2, value); } }

        double _LeanForwardBackY2 = default(double);
        public double LeanForwardBackY2 { get { return _LeanForwardBackY2; } set { base.SetProperty(ref _LeanForwardBackY2, value); } }

        double _LeanLeftRightX2 = default(double);
        public double LeanLeftRightX2 { get { return _LeanLeftRightX2; } set { base.SetProperty(ref _LeanLeftRightX2, value); } }

        double _HandDepthStdDev3 = default(double);
        public double HandDepthStdDev3 { get { return _HandDepthStdDev3; } set { base.SetProperty(ref _HandDepthStdDev3, value); } }

        double _LeanForwardBackY3 = default(double);
        public double LeanForwardBackY3 { get { return _LeanForwardBackY3; } set { base.SetProperty(ref _LeanForwardBackY3, value); } }

        double _LeanLeftRightX3 = default(double);
        public double LeanLeftRightX3 { get { return _LeanLeftRightX3; } set { base.SetProperty(ref _LeanLeftRightX3, value); } }

        private List<bool> _OnTargetList;
        public List<bool> OnTargetList { get { return _OnTargetList; } set { _OnTargetList = value; } }

        private List<bool> _IsInsideTrackForEachTickList;
        public List<bool> IsInsideTrackForEachTickList { get { return _IsInsideTrackForEachTickList; } set { _IsInsideTrackForEachTickList = value; } }

        private List<double> _AbsoluteErrorForEachTickList;
        public List<double> AbsoluteErrorForEachTickList { get { return _AbsoluteErrorForEachTickList; } set { _AbsoluteErrorForEachTickList = value; } }

        private List<double> _HandDepthForEachTickList;
        public List<double> HandDepthForEachTickList { get { return _HandDepthForEachTickList; } set { _HandDepthForEachTickList = value; } }

        private List<PointF> _LeanAmountForEachTickList;
        public List<PointF> LeanAmountForEachTickList { get { return _LeanAmountForEachTickList; } set { _LeanAmountForEachTickList = value; } }
    }
}
