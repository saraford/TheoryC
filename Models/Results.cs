using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Models
{
    public class Result : Common.BindableBase
    {
        double _TimeOnTargetMs = default(double);
        public double TimeOnTargetMs { get { return _TimeOnTargetMs; } set { base.SetProperty(ref _TimeOnTargetMs, value); } }

        double _AbsoluteError = default(double);
        public double AbsoluteError { get { return _AbsoluteError; } set { base.SetProperty(ref _AbsoluteError, value); } }

        List<double> _AbsoluteErrorForEachTickList = default(List<double>);
        public List<double> AbsoluteErrorForEachTickList { get { return _AbsoluteErrorForEachTickList; } set { base.SetProperty(ref _AbsoluteErrorForEachTickList, value); } }
    }
}
