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

    class RoutineResult : IIdentifiable, IDeletable
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
        public List<ExerciseResult> ExerciseResults { get; set; }

        [Ignore]
        Exercise[] Exercises { get; set; }
        
        public bool Deleted { get; set; }

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
            StringBuilder builder = new StringBuilder();

            if(this.EndTime != null)
            {
                TimeSpan span = (this.EndTime - this.StartTime);
                builder.Append($"{this.StartTime} ({(int)span.TotalMinutes} mins) ");
            }

            builder.AppendLine($"{Routine.Name}");

            builder.Append(ShortDetails);

            return builder.ToString();
        }

        public string ShortDetails
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                Exercise last_exercise = null;
                foreach (ExerciseResult exr in this.ExerciseResults)
                {
                    Exercise this_exercise = exr.Exercise;
                    if (this_exercise != last_exercise)
                    {
                        if(last_exercise != null)
                        {
                            builder.AppendLine();
                        }
                        
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



        public RoutineResult Transform(Routine new_routine)
        {
            // 

            if (Routine == new_routine) return this;

            RoutineResult new_rr = new RoutineResult(new_routine);

            int same_count = 0;
            int old_exercise_count = Exercises.Length;
            int new_exercise_count = new_rr.Exercises.Length;
            int max_ex_len = Math.Min(old_exercise_count, new_exercise_count);
            for(int i = 0; i < max_ex_len; i++)
            {
                if(Exercises[i] == new_rr.Exercises[i])
                {
                    same_count++;
                }
                else
                {
                    break;
                }
            }

            if(same_count < ResultCount)
            {
                throw new InvalidOperationException(
                    "This routine result cannot be transformed into new_routine");
            }

            foreach(ExerciseResult er in ExerciseResults.Take(same_count))
            {
                new_rr.ReportExerciseResult(er);
            }
            
            return new_rr;
        }

    }
}