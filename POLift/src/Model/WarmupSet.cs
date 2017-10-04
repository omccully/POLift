using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Model
{
    using Service;

    class WarmupSet
    {
        int PercentOfWeight;
        int PercentOfRestPeriod;

        public readonly int Reps;
        public readonly string Notes;

        public WarmupSet(int reps, int percent_of_weight, int percent_of_rest_period, string notes = "")
        {
            Reps = reps;
            PercentOfWeight = percent_of_weight;
            PercentOfRestPeriod = percent_of_rest_period;
            Notes = notes;
        }

        public int GetWeight(Exercise ex, int max_weight)
        {
            return Helpers.GetClosestToIncrement((max_weight * PercentOfWeight) / 100,
                ex.WeightIncrement,
                max_weight % ex.WeightIncrement);
        }

        public int GetRestPeriod(Exercise ex)
        {
            return (ex.RestPeriodSeconds * PercentOfRestPeriod) / 100;
        }
    }
}