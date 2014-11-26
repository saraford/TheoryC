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

        double _AbsoluteError = default(double);
        public double AbsoluteError { get { return _AbsoluteError; } set { base.SetProperty(ref _AbsoluteError, value); } }

        double _VariableError = default(double);
        public double VariableError { get { return _VariableError; } set { base.SetProperty(ref _VariableError, value); } }

        double _ConstantError = default(double);
        public double ConstantError { get { return _ConstantError; } set { base.SetProperty(ref _ConstantError, value); } }

        int _TickCount = default(int);
        public int TickCount { get { return _TickCount; } set { base.SetProperty(ref _TickCount, value); } }

        double _HandDepthStdDev = default(double);
        public double HandDepthStdDev { get { return _HandDepthStdDev; } set { base.SetProperty(ref _HandDepthStdDev, value); } }

        double _LeanForwardBackY = default(double);
        public double LeanForwardBackY { get { return _LeanForwardBackY; } set { base.SetProperty(ref _LeanForwardBackY, value); } }

        double _LeanLeftRightX = default(double);
        public double LeanLeftRightX { get { return _LeanLeftRightX; } set { base.SetProperty(ref _LeanLeftRightX, value); } }

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
