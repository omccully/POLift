using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using POLift.Core.Service;

namespace POLift.Core.Model
{
    public class ExerciseName : IExerciseGroup
    {
        public string Name { get; set; }
        public string ExerciseIDs { get; set; }

        public string Category { get; set; }
        public int Usage { get; set; }

        public ExerciseName(string name, string exercise_ids, string category = "other")
        {
            this.Name = name;
            this.Usage = exercise_ids.ToIDIntegers().Count();
            this.ExerciseIDs = exercise_ids;
            this.Category = category;
        }

        public ExerciseName(IEnumerable<IExercise> exercises, string default_category="other")
        {
            HashSet<int> ex_ids = new HashSet<int>();

            this.Usage = 0;
            this.Name = null;

            string highest_usage_category = null;
            foreach(IExercise ex in exercises.OrderBy(ex => ex.Usage))
            {
                ex_ids.Add(ex.ID);
                Usage += ex.Usage;

                if (this.Name == null) this.Name = ex.Name;

                if(highest_usage_category == null && !String.IsNullOrWhiteSpace(ex.Category))
                {
                    highest_usage_category = ex.Category;
                }
            }

            this.Category = String.IsNullOrWhiteSpace(highest_usage_category) ?
                default_category : highest_usage_category;
            this.ExerciseIDs = ex_ids.ToIDString();
        }

        public ExerciseName(IEnumerable<IExerciseDifficulty> difficulties,
            string default_category = "other")
        {
            HashSet<int> ex_ids = new HashSet<int>();

            string highest_usage_category = null;
            foreach (IExerciseDifficulty difficulty in difficulties.OrderByDescending(ed => ed.Usage))
            {
                int[] ids = difficulty.ExerciseIDs.ToIDIntegers();

                foreach(int id in ids)
                {
                    ex_ids.Add(id);
                }

                if (this.Name == null) this.Name = difficulty.Name;

                if (highest_usage_category == null && !String.IsNullOrWhiteSpace(difficulty.Category))
                {
                    highest_usage_category = difficulty.Category;
                }
            }

            this.Category = String.IsNullOrWhiteSpace(highest_usage_category) ?
                default_category : highest_usage_category;
            this.ExerciseIDs = ex_ids.ToIDString();
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static List<ExerciseGroupCategory> InEnCategories(IPOLDatabase Database,
            string DefaultCategory = "other")
        {
            // Key = exercise name
            // value = enumerable of EDs
            IEnumerable<IGrouping<string, ExerciseDifficulty>> groups =
                Database.Table<ExerciseDifficulty>().GroupBy(ed => ed.Name);

            List<ExerciseName> exercise_names =
                groups.Select(group => new ExerciseName(group, DefaultCategory)).ToList();

            // now group the ENs into categories
            return exercise_names.GroupBy(en => en.Category).Select(grouping =>
                new ExerciseGroupCategory(grouping.Key, 
                        grouping.OrderByDescending(g => g.Usage))
            ).ToList();
        }
    }
}
