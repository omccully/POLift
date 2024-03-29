﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public interface IRoutineResult : IDeletable, IDatabaseObject, IIdentifiable
    {
        int RoutineID { get; set; }

        IRoutine Routine { get; set; }

        DateTime StartTime { get; set; }

        DateTime EndTime { get; set; }

        string ExerciseResultsIDs { get; set; }

        List<IExerciseResult> ExerciseResults { get; set; }

        string ExerciseIDs { get; set; }

        bool Completed { get; }

        IExerciseSets CurrentExerciseSets { get; }

        IExercise NextExercise { get; }

        int ExerciseCount { get; }

        int ResultCount { get; }

        string ShortDetails { get; }

        string RelativeTimeDetails { get; }

        void ReportExerciseResult(IExerciseResult ex_result);

        IRoutineResult Transform(IRoutine new_routine, bool safe=true);

        string TimeDetails { get; }

        //void SaveEdits(Dictionary<int, float> WeightEdits, Dictionary<int, int> RepsEdits);

    }
}