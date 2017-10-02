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

    class RoutineResult : IIdentifiable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int RoutineID { get; set; }

        [Ignore]
        public Routine Routine
        {
            get
            {
                return POLDatabase.ReadByID<Routine>(RoutineID);
            }
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string ExerciseResultsIDs
        {
            get
            {
                return ExerciseResults.ToIDString();
            }
            set
            {
                ExerciseResults = POLDatabase.ParseIDString<ExerciseResult>(value).ToList();
            }
        }

        public string ExerciseIDs
        {
            get
            {
                return Exercises.ToIDString();
            }
            set
            {
                Exercises = POLDatabase.ParseIDString<Exercise>(value).ToArray();
            }
        }

        [Ignore]
        public bool Completed
        {
            get
            {
                // idk why ExerciseResults.Count would ever be
                // GREATER than the exercise length, but just in case
                return ExerciseResults.Count >= Exercises.Length;
            }
        }

        [Ignore]
        List<ExerciseResult> ExerciseResults { get; set; }

        [Ignore]
        Exercise[] Exercises { get; set; }
        //int next_exercise_index = 0;

        public RoutineResult(Routine Routine) : this(Routine.ID)
        {
            
        }

        public RoutineResult(int RoutineID)
        {
            this.RoutineID = RoutineID;
            Exercises = Routine.Exercises.ToArray();
            ExerciseResults = new List<ExerciseResult>();
        }

        public RoutineResult()
        {
            ExerciseResults = new List<ExerciseResult>();
        }

        [Ignore]
        public Exercise NextExercise
        {
            get
            {
                if(Completed)
                {
                    return null;
                }

                return Exercises[ExerciseResults.Count()];
            }
        }

        [Ignore]
        public int ExerciseCount
        {
            get
            {
                return Exercises.Length;
            }
        }

        [Ignore]
        public int ResultCount
        {
            get
            {
                return ExerciseResults.Count();
            }
        }

        public void ReportExerciseResult(ExerciseResult ex_result)
        {
            if (Completed)
            {
                throw new InvalidOperationException("Too many exercise results reported");
            }

            if(NextExercise != ex_result.Exercise)
            {
                throw new ArgumentException("ExerciseResult is not for the correct exercise. " +
                    "Use NextExercise to retrieve the expected exercise.");
            }

            if (ResultCount == 0) StartTime = ex_result.Time;
            
            EndTime = ex_result.Time;

            ExerciseResults.Add(ex_result);
        }

        public static RoutineResult MostRecentForRoutine(Routine r)
        {
            try
            {
                return POLDatabase.Table<RoutineResult>()
                    .Where(rr => rr.RoutineID == r.ID)
                    .MaxObject(rr => rr.EndTime);
            }
            catch(InvalidOperationException)
            {
                return null;
            }
        }

        public static RoutineResult MostRecentUncompleted(Routine r)
        {
            RoutineResult rr = MostRecentForRoutine(r);
            if (rr == null) return null;
            if (rr.Completed) return null;
            return rr;
        }

        public override string ToString()
        {
            //return $"routine #{RoutineID} exercises={this.ExerciseIDs} exerciseresults={this.ExerciseResultsIDs} endtime={this.EndTime}";

            StringBuilder builder = new StringBuilder();
            builder.Append(this.StartTime + " " + Routine.Name);

            Exercise last_exercise = null;
            foreach(ExerciseResult exr in this.ExerciseResults)
            {
                Exercise this_exercise = exr.Exercise;
                if(this_exercise != last_exercise)
                {
                    builder.AppendLine();
                    builder.Append(this_exercise.Name + " "); 
                }
                else
                {
                    builder.Append(", ");
                }

                builder.Append($"{exr.Weight}x{exr.RepCount}");

                last_exercise = this_exercise;
            }

            return builder.ToString();
        }

    }
}