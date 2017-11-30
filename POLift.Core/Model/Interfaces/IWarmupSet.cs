using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public interface IWarmupSet
    {
        int Reps { get; }
        string Notes { get; }

        float GetWeight(IExercise ex, float max_weight);

        int GetRestPeriod(IExercise ex);
    }
}