using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class ExerciseCategory
    {
        public readonly string Name;
        public readonly List<IExercise> Exercises;

        public ExerciseCategory(string name, IEnumerable<IExercise> exercises)
        {
            this.Name = name;
            this.Exercises = new List<IExercise>(exercises);
        }
    }
}
