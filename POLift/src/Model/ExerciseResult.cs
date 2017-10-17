using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite;

namespace POLift.Model
{
    using Service;

    class ExerciseResult : IExerciseResult, IIdentifiable
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int ExerciseID { get; set; }

        [Ignore]
        public IExercise Exercise
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

        int _Weight;
        public int Weight
        {
            get
            {
                return _Weight;
            }
            set
            {
                if(value < 0)
                {
                    throw new ArgumentException("Weight must be non-negative");
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

        public bool Deleted { get; set; } = false;

        public ExerciseResult(IExercise Exercise, int Weight, int RepCount) : 
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

        public static ExerciseResult MostRecentResultOf(IPOLDatabase database, Exercise exercise)
        {
            try
            {
                return database.Table<ExerciseResult>()
                    .Where(er => er.ExerciseID == exercise.ID)
                    .MaxObject(er => er.Time);
            }
            catch(InvalidOperationException)
            {
                return null;
            }
            
        }

        public override string ToString()
        {
            return $"exercise #{ExerciseID}, {Weight} weight, {RepCount} reps on {Time}";
        }

        public static Dictionary<int, int> Import(IEnumerable<ExerciseResult> exercise_results,
           IPOLDatabase destination, Dictionary<int, int> ExercisesLookup)
        {
            Dictionary<int, int> ExerciseResultLookup = new Dictionary<int, int>();
            // loop through exercise results
            // swap the ExerciseID for equivalent for the existing db
            foreach (ExerciseResult exercise_result in exercise_results)
            {
                int old_id = exercise_result.ID;

                exercise_result.ExerciseID = ExercisesLookup[exercise_result.ExerciseID];
                exercise_result.ID = 0;
                destination.InsertOrUpdateNoID(exercise_result);

                ExerciseResultLookup[old_id] = exercise_result.ID;
            }
            return ExerciseResultLookup;
        }
    }
}