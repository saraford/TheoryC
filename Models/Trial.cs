﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Models
{
    public class Trial : Common.BindableBase
    {

        int _shapeSizeDiameter = default(int);
        public int ShapeSizeDiameter { get { return _shapeSizeDiameter; } set { base.SetProperty(ref _shapeSizeDiameter, value); } }

        int _durationSeconds = default(int);
        public int DurationSeconds { get { return _durationSeconds; } set { base.SetProperty(ref _durationSeconds, value); } }

        int _number = default(int);
        public int Number { get { return _number; } set { base.SetProperty(ref _number, value); } }

        int _TimeOnTargetMs = default(int);
        public int TimeOnTargetMs { get { return _TimeOnTargetMs; } set { base.SetProperty(ref _TimeOnTargetMs, value); } }

        Models.Result _Results = default(Models.Result);
        public Models.Result Results { get { return _Results; } set { _Results = value; } }

        public Trial()
        {
            _shapeSizeDiameter = 50;
            _durationSeconds = 5;
        }

    }
}
