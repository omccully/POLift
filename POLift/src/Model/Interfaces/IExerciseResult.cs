using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    public interface IExerciseResult : IIdentifiable, IDatabaseObject, IDeletable
    {
        int ExerciseID { get; set; }

        IExercise Exercise { get; set; }

        int Weight { get; set; }

        int RepCount { get; set; }

        DateTime Time { get; set; }



    }
}