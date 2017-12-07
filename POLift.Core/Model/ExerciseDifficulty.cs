using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite.Net.Attributes;

namespace POLift.Core.Model
{
    using Service;

    public class ExerciseDifficulty : IExerciseDifficulty, IIdentifiable, IDatabaseObject
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
        [Indexed(Name = "UniqueGroupExerciseDifficulty", Order = 1, Unique = true)]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The exercise must have a name");
                }
                this._Name = value.ToLower();
            }
        }

        int _RestPeriodSeconds;
        [Indexed(Name = "UniqueGroupExerciseDifficulty", Order = 2, Unique = true)]
        public int RestPeriodSeconds
        {
            get
            {
                return _RestPeriodSeconds;
            }
            set
            {
                const int MAX_REST_PERIOD_MINUTES = 10;
                const int MAX_REST_PERIOD = MAX_REST_PERIOD_MINUTES * 60;
                if (value < 0 || value > MAX_REST_PERIOD)
                {
                    throw new ArgumentException($"Rest period must be within 0 seconds and {MAX_REST_PERIOD_MINUTES} minutes");
                }
                this._RestPeriodSeconds = value;
            }
        }

        string _ExerciseIDs;
        public string ExerciseIDs
        {
            get
            {
                return _ExerciseIDs;
            }
            set
            {
                _ExerciseIDs = value;
            }
        }

        [Ignore]
        public IEnumerable<Exercise> Exercises
        {
            get
            {
                // read exercises from DB based on the IDs
                if (ExerciseIDs == null)
                {
                    return new List<Exercise>();
                }

                try
                {
                    return Database.ParseIDString<Exercise>(ExerciseIDs);
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("ex ids: " + ExerciseIDs);
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    throw e;
                }
                
            }
            set
            {
                ExerciseIDs = value.ToIDString();
            }
        }

        string _Category;
        public string Category
        {
            get
            {
                return _Category;
            }
            set
            {
                if (value == null)
                {
                    _Category = null;
                }
                else
                {
                    _Category = Helpers.UniformString(value);
                }
            }
        }

        int _Usage = 1;
        public int Usage
        {
            get
            {
                //if (_Usage == 0)
                //{
                //    RefreshUsage();
                //}

                return _Usage;
            }
            set
            {
                _Usage = value;
            }
        }

        public ExerciseDifficulty()
        {

        }

        public ExerciseDifficulty(IExercise ex)
        {
            this.Name = ex.Name;
            this.RestPeriodSeconds = ex.RestPeriodSeconds;
            this.Category = ex.Category;
            this.ExerciseIDs = ex.ID.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>True if just added, false if already contained</returns>
        public bool AddExercise(IExercise ex)
        {
            int[] ids_array = ExerciseIDs.ToIDIntegers();

            if (ids_array.Contains(ex.ID)) return false;

            int[] new_ids_array = new int[ids_array.Length + 1];
            ids_array.CopyTo(new_ids_array, 0);
            new_ids_array[ids_array.Length] = ex.ID;

            ExerciseIDs = new_ids_array.ToIDString();

            return true;
        }

        int CalculateUsage()
        {
            return this.Exercises.Sum(e => e.Usage);
        }

        public bool RefreshUsage()
        {
            int old_usage = _Usage;
            _Usage = CalculateUsage();
            return old_usage != _Usage;
        }

        public static void RefreshAllUsages(IPOLDatabase database)
        {
            IEnumerable<ExerciseDifficulty> eds =
                database.Table<ExerciseDifficulty>().Where(e => e.Usage == 0);

            foreach (ExerciseDifficulty ed in eds)
            {
                ed.RefreshUsage();
                database.Update(ed);
            }
        }

        public static int Regenerate(IPOLDatabase database)
        {
            foreach(Exercise ex in database.Table<Exercise>())
            {
                ExerciseDifficulty difficulty = ex.GetDifficultyRecord();

                if (difficulty == null)
                {
                    difficulty = new ExerciseDifficulty(ex);
                    difficulty.Database = database;
                    database.Insert(difficulty);
                }
                else
                {
                    if (difficulty.AddExercise(ex))
                    {
                        database.Update(difficulty);
                    }
                }
            }

            return 0;
        }

        public override string ToString()
        {
            string ret = $"{this.Name}, {this.RestPeriodSeconds} sec rest";

#if DEBUG
            //ret += " " + Usage;
#endif

            return ret;
        }

        public static List<KeyValuePair<string, List<IExerciseDifficulty>>> InCategories(
            IPOLDatabase Database,
            string DefaultCategory = "other")
        {
            Dictionary<string, List<IExerciseDifficulty>> dict =
                new Dictionary<string, List<IExerciseDifficulty>>();

            foreach (ExerciseDifficulty ex in Database
                .Table<ExerciseDifficulty>().OrderByDescending(ed => ed.Usage))
            {
                string cat = (ex.Category == null ? DefaultCategory : ex.Category);

                if (dict.ContainsKey(cat))
                {
                    dict[cat].Add(ex);
                }
                else
                {
                    dict[cat] = new List<IExerciseDifficulty>() { ex };
                }
            }

            //POLDatabase.Table<Exercise>()
            //    .GroupBy(ex => ex.Category)
            //    .OrderByDescending(group => group.Count());


            return dict.OrderByDescending(kvp =>
                kvp.Value.Count
            ).ToList();
        }
    }
}