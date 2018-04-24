using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class ExerciseNameCategory
    {
        public readonly string Name;
        public readonly List<ExerciseName> ExerciseNames;

        public ExerciseNameCategory(string name,
            IEnumerable<ExerciseName> exercise_names)
        {
            this.Name = name;
            this.ExerciseNames =
                new List<ExerciseName>(exercise_names);
        }

    }
}
