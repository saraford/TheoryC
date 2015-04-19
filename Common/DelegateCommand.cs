/*  Copyright (c)2015 San Jose State University - All Rights Reserved
    Licensed under the Microsoft Public License (Ms-Pl)
    Created by Sara Ford and Dr. Emily Wughalter, Dept of Kinesiology, San Jose State University */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheoryC.Common
{
    public class DelegateCommand : System.Windows.Input.ICommand
    {
        private readonly Action m_Execute;
        private readonly Func<bool> m_CanExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute)
            : this(execute, () => true) { /* empty */ }

        public DelegateCommand(Action execute, Func<bool> canexecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            m_Execute = execute;
            m_CanExecute = canexecute;
        }

        public bool CanExecute(object p)
        {
            return m_CanExecute == null ? true : m_CanExecute();
        }

        public void Execute(object p)
        {
            if (CanExecute(null))
                m_Execute();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
