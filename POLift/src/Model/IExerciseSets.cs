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
    interface IExerciseSets : IIdentifiable, IDatabaseObject
    {
        int SetCount { get; set; }

        int ExerciseID { get; set; }

        Exercise Exercise { get; set; }
    }
}