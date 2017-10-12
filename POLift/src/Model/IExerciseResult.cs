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
    interface IExerciseResult : IIdentifiable, IDatabaseObject
    {
        int ExerciseID { get; set; }

        IExercise Exercise { get; set; }

        int Weight { get; set; }

        int RepCount { get; set; }

        DateTime Time { get; set; }



    }
}