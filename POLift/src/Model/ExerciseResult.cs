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

    class ExerciseResult : IIdentifiable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int ExerciseID { get; set; }

        [Ignore]
        public Exercise Exercise
        {
            get
            {
                return POLDatabase.ReadByID<Exercise>(ExerciseID);
            }
        }

        int _Weight;
        public int Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                if(value <= 0)
                {
                    throw new ArgumentException("Weight must be positive");
                }
                _Weight = value;
            }
        }

        int _RepCount;
        public int RepCount
        {
            get
            {
                return _RepCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Rep count must be non-negative");
                }
                _RepCount = value;
            }
        }

        public DateTime Time { get; set; }

        public ExerciseResult(Exercise Exercise, int Weight, int RepCount) : 
            this(Exercise.ID, Weight, RepCount)
        {
        }

        public ExerciseResult(int ExerciseID, int Weight, int RepCount)
        {
            this.ExerciseID = ExerciseID;
            this.Weight = Weight;
            this.RepCount = RepCount;
            Time = DateTime.Now;
        }

        public ExerciseResult()
        {

        }

        public static ExerciseResult MostRecentResultOf(Exercise exercise)
        {
            try
            {
                return POLDatabase.Table<ExerciseResult>()
                    .Where(er => er.ExerciseID == exercise.ID)
                    .MaxObject(er => er.Time);
            }
            catch(InvalidOperationException)
            {
                return null;
            }
            
        }
    }
}