﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    using Model;
    public interface IPerformWarmupViewModel : IValueReturner<float>
    {
        IExercise WarmupExercise { get; set; }
    }
}
