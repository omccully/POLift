using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite;

namespace POLift.Model
{
    using Service;

    /// <summary>
    /// Represents a series of sets of the same exercise.
    /// ex: 3 sets of the exercise "bench, 6 max reps, 120 second rest, 5 weight increment"
    /// </summary>
    class ExerciseSets : IExerciseSets, IIdentifiable
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        const int DEFAULT_SET_COUNT = 3;

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        int _SetCount;
        [Indexed(Name = "UniqueGroupExerciseSets", Order = 1, Unique = true)]
        public int SetCount
        {
            get
            {
                return _SetCount;
            }
            set
            {
                //if (value < 1) throw new ArgumentException("Set count must be at least 1");
                if (value <= 0)
                {
                    _SetCount = 0;
                }
                else
                {
                    _SetCount = value;
                }
            }
        }

        [Indexed(Name = "UniqueGroupExerciseSets", Order = 2, Unique = true)]
        public int ExerciseID { get; set; }

        [Ignore]
        public Exercise Exercise
        {
            get
            {
                return Database.ReadByID<Exercise>(ExerciseID);
            }
            set
            {
                ExerciseID = value.ID;
            }
        }

        public ExerciseSets(IExercise exercise, int SetCount = DEFAULT_SET_COUNT,
            IPOLDatabase database = null) : this(exercise.ID, SetCount, database)
        {

        }

        public ExerciseSets(int ExerciseID, int SetCount = DEFAULT_SET_COUNT, IPOLDatabase database = null)
        {
            this.ExerciseID = ExerciseID;
            this.SetCount = SetCount;
            this.Database = database;
        }

        public ExerciseSets() : this(0)
        {

        }

        public override string ToString()
        {
            return $"{this.SetCount} sets of {this.Exercise.Name}";
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Routine return false.
            ExerciseSets r = obj as ExerciseSets;

            return this.Equals(r);
        }

        public bool Equals(ExerciseSets r)
        {
            if ((System.Object)r == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.SetCount == r.SetCount &&
                this.ExerciseID == r.ExerciseID);
        }

        public static bool operator ==(ExerciseSets a, ExerciseSets b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(ExerciseSets a, ExerciseSets b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.ExerciseID ^ this.SetCount;
        }

        public static List<IExercise> Expand(IEnumerable<IExerciseSets> exercise_sets)
        {
            List<IExercise> results = new List<IExercise>();

            foreach (IExerciseSets sets in exercise_sets)
            {
                IExercise ex = sets.Exercise;

                for (int i = 0; i < sets.SetCount; i++)
                {
                    results.Add(ex);
                }
            }

            return results;
        }
    
        public static List<IExerciseSets> Group(IEnumerable<IExercise> exercises, IPOLDatabase database)
        {
            List<IExerciseSets> exercise_sets = new List<IExerciseSets>();

            IExercise last_ex = null;
            int set_count = 1;
            foreach (IExercise ex in exercises)
            {
                if (ex.Equals(last_ex))
                {
                    set_count++;
                }
                else if (last_ex != null)
                {
                    exercise_sets.Add(new ExerciseSets(last_ex, set_count, database));
                    set_count = 1; // number of sets of ex
                }
                last_ex = ex;
            }

            if (last_ex != null)
            {
                exercise_sets.Add(new ExerciseSets(last_ex, set_count, database));
            }

            return exercise_sets;
        }

        public static List<IExerciseSets> Regroup(IEnumerable<IExerciseSets> exercise_sets, IPOLDatabase database)
        {
            return Group(Expand(exercise_sets), database);
        }

        public static Dictionary<int, int> Import(IEnumerable<ExerciseSets> exercise_setss, 
            IPOLDatabase destination, Dictionary<int, int> ExerciseLookup)
        {
            Dictionary<int, int> ExerciseSetsLookup = new Dictionary<int, int>();

            // loop through exercisesets
            // swap the ExerciseIDs with the equivalent for the existing db

            foreach (ExerciseSets exercise_sets in exercise_setss)
            {
                int old_id = exercise_sets.ID;
                exercise_sets.ExerciseID = ExerciseLookup[exercise_sets.ExerciseID];
                exercise_sets.ID = 0;

                destination.InsertOrUpdateNoID(exercise_sets);

                ExerciseSetsLookup[old_id] = exercise_sets.ID;
            }

            return ExerciseSetsLookup;
        }

        public static Dictionary<int, int> PruneByConstaints(IPOLDatabase dab,
            Dictionary<int, int> ExerciseLookup)
        {
            Dictionary<int, int> ExerciseSetsMapping = new Dictionary<int, int>();
            HashSet<ExerciseSets> existing_exercise_sets = new HashSet<ExerciseSets>();

            foreach (ExerciseSets exercise_sets in dab.Table<ExerciseSets>())
            {
                if (existing_exercise_sets.Contains(exercise_sets))
                {
                    dab.Delete<ExerciseSets>(exercise_sets.ID);
                }
                else
                {
                    if(ExerciseLookup.ContainsKey(exercise_sets.ExerciseID))
                    {
                        exercise_sets.ExerciseID = 
                            ExerciseLookup[exercise_sets.ExerciseID];
                        dab.Update((ExerciseSets)exercise_sets);
                    }
                }
            }

            return ExerciseSetsMapping;
        }
    }
}
