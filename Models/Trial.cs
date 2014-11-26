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
            _durationSeconds = 1;
            _rpm = 30; 
        }

        internal void ClearResults()
        {
            Results.AbsoluteError = 0;
            Results.AbsoluteErrorForEachTickList.Clear();
            Results.TimeOnTarget = 0;
            Results.IsInsideTrackForEachTickList.Clear();
            Results.ConstantError = 0;
            Results.TickCount = 0;
            Results.VariableError = 0;
            Results.HandDepthForEachTickList.Clear();
        }
    }
}
