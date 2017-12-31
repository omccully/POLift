using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class ExerciseDifficultyCategory
    {
        public readonly string Name;
        public readonly List<IExerciseDifficulty> ExerciseDifficulties;

        public ExerciseDifficultyCategory(string name, 
            IEnumerable<IExerciseDifficulty> exercise_difficulties)
        {
            this.Name = name;
            this.ExerciseDifficulties =
                new List<IExerciseDifficulty>(exercise_difficulties);
        }
    }
}
