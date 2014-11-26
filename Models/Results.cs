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

        List<bool> _IsInsideTrackForEachTickList = default(List<bool>);
        public List<bool> IsInsideTrackForEachTickList { get { return _IsInsideTrackForEachTickList; } set { base.SetProperty(ref _IsInsideTrackForEachTickList, value); } }

        int _TickCount = default(int);
        public int TickCount { get { return _TickCount; } set { base.SetProperty(ref _TickCount, value); } }

        double _HandDepth = default(double);
        public double HandDepth { get { return _HandDepth; } set { base.SetProperty(ref _HandDepth, value); } }

        List<double> _AbsoluteErrorForEachTickList = default(List<double>);
        public List<double> AbsoluteErrorForEachTickList { get { return _AbsoluteErrorForEachTickList; } set { base.SetProperty(ref _AbsoluteErrorForEachTickList, value); } }

        private List<double> _HandDepthForEachTickList;
        public List<double> HandDepthForEachTickList { get { return _HandDepthForEachTickList; } set { _HandDepthForEachTickList = value; } }

    }
}
