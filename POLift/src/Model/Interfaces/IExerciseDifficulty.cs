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
    interface IExerciseDifficulty : IIdentifiable, IDatabaseObject
    {
        string Name { get; set; }
        int RestPeriodSeconds { get; set; }
        string ExerciseIDs { get; set; }
        IEnumerable<Exercise> Exercises { get; set; }
        int Usage { get; set; }

        bool RefreshUsage();

        bool AddExercise(IExercise ex);
    }
}