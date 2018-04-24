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

        public ExerciseName(string name, string exercise_ids)
        {
            this.Name = name;
            this.ExerciseIDs = exercise_ids;
        }

        public ExerciseName(IEnumerable<IExercise> exercises, string default_category="other")
        {
            HashSet<int> ex_ids = new HashSet<int>();

            int highest_usage = 0;
            string highest_usage_category = "";
            foreach(IExercise ex in exercises)
            {
                ex_ids.Add(ex.ID);

                if(ex.Usage >= highest_usage)
                {
                    this.Name = ex.Name;
                    highest_usage = ex.Usage;
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

            int highest_usage = 0;
            string highest_usage_category = "";
            foreach (IExerciseDifficulty difficulty in difficulties)
            {
                int[] ids = difficulty.ExerciseIDs.ToIDIntegers();

                foreach(int id in ids)
                {
                    ex_ids.Add(id);
                }

                if (difficulty.Usage >= highest_usage)
                {
                    this.Name = difficulty.Name;
                    highest_usage = difficulty.Usage;
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
                new ExerciseGroupCategory(grouping.Key, grouping)).ToList();
        }
    }
}
