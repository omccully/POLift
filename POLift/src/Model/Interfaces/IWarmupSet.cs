using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    public interface IWarmupSet
    {
        int Reps { get; }
        string Notes { get; }

        int GetWeight(IExercise ex, int max_weight);

        int GetRestPeriod(IExercise ex);
    }
}