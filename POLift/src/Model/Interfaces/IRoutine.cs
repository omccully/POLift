using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    public interface IRoutine : IIdentifiable, IDeletable, IDatabaseObject
    {
        string Name { get; set; }

        IEnumerable<IExerciseSets> ExerciseSets { get; set; }

        string ExerciseSetIDs { get; set; }

        List<IExercise> Exercises { get; }
    }
}