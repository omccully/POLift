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
    interface IRoutineResult : IDatabaseObject
    {
        int RoutineID { get; set; }

        IRoutine Routine { get; set; }

        DateTime StartTime { get; set; }

        DateTime EndTime { get; set; }

        string ExerciseResultsIDs { get; set; }

        string ExerciseIDs { get; set; }

        bool Completed { get; set; }

        IExercise NextExercise { get; set; }

        int ExerciseCount { get; set; }

        int ResultCount { get; set; }

        string ShortDetails { get; set; }

        void ReportExerciseResult(IExerciseResult ex_result);

        IRoutineResult Transform(IRoutine new_routine);

    }
}