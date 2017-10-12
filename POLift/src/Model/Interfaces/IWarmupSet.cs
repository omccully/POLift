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
    interface IWarmupSet
    {
        int Reps { get; private set; }
        int Notes { get; private set; }

        int GetWeight(IExercise ex, int max_weight);

        int GetRestPeriod(IExercise ex);
    }
}