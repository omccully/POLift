﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    public interface IValueReturner<T>
    {
        event Action<T> ValueChosen;
    }
}
