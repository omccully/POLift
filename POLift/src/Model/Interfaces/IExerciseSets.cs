using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    public interface IExerciseSets : IIdentifiable, IDatabaseObject
    {
        int SetCount { get; set; }

        int ExerciseID { get; set; }

        Exercise Exercise { get; set; }
    }
}