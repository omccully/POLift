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
        public int SetCount
        {
            get
            {
                return _SetCount;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Set count must be at least 1");
                _SetCount = value;
            }
        }

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
    }
}