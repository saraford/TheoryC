using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Models
{
    public class Result : Common.BindableBase
    {
        int _TimeOnTargetMS = default(int);
        public int TimeOnTargetMS { get { return _TimeOnTargetMS; } set { base.SetProperty(ref _TimeOnTargetMS, value); } }

        double _AbsoluteError = default(double);
        public double AbsoluteError { get { return _AbsoluteError; } set { base.SetProperty(ref _AbsoluteError, value); } }
    
    }
}
