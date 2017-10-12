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
    interface IRoutine : IIdentifiable, IDeletable, IDatabaseObject
    {
        string Name { get; set; }

        IEnumerable<IExerciseSets> ExerciseSets { get; set; }

        string ExerciseSetIDs { get; set; }

        List<IExercise> Exercises { get; set; }
    }
}