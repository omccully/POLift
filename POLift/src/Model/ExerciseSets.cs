using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

namespace POLift.Model
{
    using Service;

    /// <summary>
    /// Represents a series of sets of the same exercise.
    /// ex: 3 sets of the exercise "bench, 6 max reps, 120 second rest, 5 weight increment"
    /// </summary>
    class ExerciseSets : IIdentifiable
    {
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
                if(value <= 0)
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
                return POLDatabase.ReadByID<Exercise>(ExerciseID);
            }
            set
            {
                ExerciseID = value.ID;
            }
        }

        public ExerciseSets(Exercise exercise, int SetCount = DEFAULT_SET_COUNT) : this(exercise.ID, SetCount)
        {

        }

        public ExerciseSets(int ExerciseID, int SetCount = DEFAULT_SET_COUNT)
        {
            this.ExerciseID = ExerciseID;
            this.SetCount = SetCount;
        }

        public ExerciseSets() : this(0)
        {

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

        public static List<ExerciseSets> Group(IEnumerable<Exercise> exercises)
        {
            List<ExerciseSets> exercise_sets = new List<ExerciseSets>();

            Exercise last_ex = null;
            int set_count = 1;
            foreach(Exercise ex in exercises)
            {
                if(ex == last_ex)
                {
                    set_count++;
                }
                else if(last_ex != null)
                {
                    exercise_sets.Add(new ExerciseSets(last_ex, set_count));
                    set_count = 1; // number of sets of ex
                }
                last_ex = ex;
            }

            if(last_ex != null)
            {
                exercise_sets.Add(new ExerciseSets(last_ex, set_count));
            }

            return exercise_sets;
        }
    }
}