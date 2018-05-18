using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public interface IExerciseDifficulty : IExerciseGroup, IIdentifiable, IDatabaseObject
    {
        //string Name { get; set; }
        int RestPeriodSeconds { get; set; }
        //string ExerciseIDs { get; set; }
        IEnumerable<Exercise> Exercises { get; set; }
        //int Usage { get; set; }
        //string Category { get; set; }

        bool RefreshUsage();

        bool AddExercise(IExercise ex);
    }
}