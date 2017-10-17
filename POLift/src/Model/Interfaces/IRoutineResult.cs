using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
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

        IExercise NextExercise { get; }

        int ExerciseCount { get; }

        int ResultCount { get; }

        string ShortDetails { get; }

        void ReportExerciseResult(IExerciseResult ex_result);

        IRoutineResult Transform(IRoutine new_routine);

    }
}