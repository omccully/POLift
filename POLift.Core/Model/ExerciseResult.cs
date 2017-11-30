using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite.Net.Attributes;

namespace POLift.Core.Model
{
    using Service;

    public class ExerciseResult : IExerciseResult, IIdentifiable
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

        float _Weight;
        public float Weight
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

        public ExerciseResult(IExercise Exercise, float Weight, int RepCount) : 
            this(Exercise.ID, Weight, RepCount)
        {
        }

        public ExerciseResult(int ExerciseID, float Weight, int RepCount)
        {
            this.ExerciseID = ExerciseID;
            this.Weight = Weight;
            this.RepCount = RepCount;
            Time = DateTime.Now;
        }

        public ExerciseResult()
        {

        }

        public static IEnumerable<ExerciseResult> LastNOfExercise(
            IPOLDatabase database, IExercise exercise, int count)
        {
            return database.Table<ExerciseResult>()
                .Where(er => er.ExerciseID == exercise.ID)
                .OrderByDescending(er => er.Time)
                .Take(count);
        }

        public static ExerciseResult MostRecentResultOf(IPOLDatabase database, IExercise exercise)
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

        public bool IsSuccess(float most_recent_weight, IExercise this_exercise=null)
        {
            if(this_exercise == null)
            {
                this_exercise = this.Exercise;
            }
                
            return this.RepCount >= this_exercise.MaxRepCount &&
                this.Weight >= most_recent_weight;
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

        public static int TranslateExerciseIDs(IPOLDatabase dab,
            Dictionary<int, int> ExerciseLookup)
        {
            int count = 0;
            foreach (ExerciseResult rr in dab.Table<ExerciseResult>())
            {
                if (ExerciseLookup.ContainsKey(rr.ExerciseID))
                {
                    rr.ExerciseID = ExerciseLookup[rr.ExerciseID];
                    dab.Update((ExerciseResult)rr);
                    count++;
                }
            }
            return count;
        }
    }
}