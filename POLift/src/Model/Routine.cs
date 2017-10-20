using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using SQLite;

namespace POLift.Model
{
    using Service;

    class Routine : IRoutine, IIdentifiable, IDeletable
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
        [Indexed(Name = "UniqueGroupRoutine", Order = 1, Unique = true)]
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
                    throw new ArgumentException("The routine must have a name");
                }
                this._Name = value;
            }
        }


        [Ignore]
        public IEnumerable<IExerciseSets> ExerciseSets
        {
            get
            {
                // read exercises from DB based on the IDs
                if (ExerciseSetIDs == null)
                {
                    return new List<ExerciseSets>();
                }

                return Database.ParseIDString<ExerciseSets>(ExerciseSetIDs);
            }
            set
            {
                ExerciseSetIDs = value.Where(ex_set => ex_set.SetCount > 0).ToIDString();
            }
        }

        string _ExerciseSetIDs;
        [Indexed(Name = "UniqueGroupRoutine", Order = 2, Unique = true)]
        public string ExerciseSetIDs
        {
            get
            {
                return _ExerciseSetIDs;
            }
            set
            {
                _ExerciseSetIDs = value;
            }
        }

        public bool Deleted { get; set; } = false;

        public Routine(string Name, string exercise_set_ids)
        {
            this.Name = Name;
            ExerciseSetIDs = exercise_set_ids;
        }

        public Routine(string Name, IEnumerable<IExerciseSets> exercise_sets)
        {
            this.Name = Name;
            ExerciseSets = exercise_sets;
        }

        public Routine() : this("Generic routine", "")
        {

        }

        public override string ToString()
        {
            int exercises = ExerciseSets.Count();
            int total_sets = ExerciseSets.Sum(e => e.SetCount);

            return $"{Name}, {exercises} exercises, {total_sets} sets (ID {ID})";
        }

        [Ignore]
        public List<IExercise> Exercises
        {
            get
            {
                List<IExercise> results = new List<IExercise>();

                foreach (IExerciseSets sets in this.ExerciseSets)
                {
                    IExercise ex = sets.Exercise;

                    for (int i = 0; i < sets.SetCount; i++)
                    {
                        results.Add(ex);
                    }
                }

                return results;
            }
        }

        public string RecentResultDetails
        {
            get
            {
                IRoutineResult rr = RoutineResult.MostRecentForRoutine(Database, this);

                if (rr == null) return "Never performed";

                if (rr.Completed)
                {
                    // TODO: change this to days/hours/minutes ago
                    TimeSpan time_since_end = DateTime.Now - rr.EndTime;

                    return $"Last completed {time_since_end.ToRoundedString()} ago";
                }

                TimeSpan time_since_start = DateTime.Now - rr.StartTime;

                return $"Uncompleted {time_since_start.ToRoundedString()} ago";
            }
        }

        public string ExtendedDetails
        {
            get
            {
                StringBuilder result = new StringBuilder();
                result.AppendLine(this.ToString());
                result.AppendLine();

                foreach (IExerciseSets ex_sets in this.ExerciseSets)
                {
                    result.AppendLine($"{ex_sets.SetCount} x {ex_sets.Exercise.ToString()}");
                    result.AppendLine();

                }

                return result.ToString();
            }
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Routine return false.
            Routine r = obj as Routine;

            return this.Equals(r);
        }

        public bool Equals(Routine r)
        {
            if ((System.Object)r == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.ExerciseSetIDs == r.ExerciseSetIDs &&
                this.Name == r.Name);
        }

        public static bool operator ==(Routine a, Routine b)
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

        public static bool operator !=(Routine a, Routine b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.ExerciseSetIDs.GetHashCode() ^ this.Name.GetHashCode();
        }


        public static Dictionary<int, int> Import(IEnumerable<Routine> routines,
            IPOLDatabase destination, Dictionary<int, int> ExerciseSetsLookup)
        {
            Dictionary<int, int> RoutineLookup = new Dictionary<int, int>();
            // loop through routines
            // swap the ExerciseSetIDs for equivalent for the existing db
            foreach (Routine routine in routines)
            {
                int old_id = routine.ID;

                //routine.ExerciseSetIDs = routine.ExerciseSetIDs.ToIDIntegers()

                routine.ExerciseSetIDs = 
                    Helpers.TranslateIDString(routine.ExerciseSetIDs, ExerciseSetsLookup);
                routine.ID = 0;

                destination.InsertOrUpdateNoID(routine);

                RoutineLookup[old_id] = routine.ID;
            }
            return RoutineLookup;
        }
    }
}