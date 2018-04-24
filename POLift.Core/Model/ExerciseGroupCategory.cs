using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class ExerciseGroupCategory
    {
        public readonly string Name;
        public readonly List<IExerciseGroup> ExerciseGroups;

        public ExerciseGroupCategory(string name,
            IEnumerable<IExerciseGroup> exercise_names)
        {
            this.Name = name;
            this.ExerciseGroups =
                new List<IExerciseGroup>(exercise_names);
        }
    }
}
