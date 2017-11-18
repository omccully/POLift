using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    using Service;

    class WarmupSet : IWarmupSet
    {
        int PercentOfWeight;
        int PercentOfRestPeriod;

        public int Reps { get; private set; }
        public string Notes { get; private set; }

        public WarmupSet(int reps, int percent_of_weight, int percent_of_rest_period, string notes = "")
        {
            Reps = reps;
            PercentOfWeight = percent_of_weight;
            PercentOfRestPeriod = percent_of_rest_period;
            Notes = notes;
        }

        public float GetWeight(IExercise ex, float max_weight)
        {
            return Helpers.GetClosestToIncrement((max_weight * PercentOfWeight) / 100,
                ex.WeightIncrement,
                max_weight % ex.WeightIncrement);
        }

        public int GetRestPeriod(IExercise ex)
        {
            return (ex.RestPeriodSeconds * PercentOfRestPeriod) / 100;
        }
    }
}