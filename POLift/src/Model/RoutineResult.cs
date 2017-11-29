using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite;

namespace POLift.Model
{
    using Service;

    class RoutineResult : IRoutineResult, IIdentifiable, IDeletable
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int RoutineID { get; set; }

        [Ignore]
        public IRoutine Routine
        {
            get
            {
                return Database.ReadByID<Routine>(RoutineID);
            }
            set
            {
                RoutineID = value.ID;
            }
        }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        string _ExerciseResultsIDs = "";
        public string ExerciseResultsIDs
        {
            get
            {
                //return ExerciseResults.ToIDString();

                if (_ExerciseResults != null)
                {
                    // client requested ExerciseResults, so it may have been changed since 
                    // the object was 

                    return _ExerciseResults.ToIDString();
                }

                return _ExerciseResultsIDs;
            }
            set
            {

                _ExerciseResultsIDs = value;

                if (_ExerciseResults != null)
                {
                    // client requested ExerciseResults, so it may have been changed since 
                    // the object was 

                    // invalidate the cached object
                    _ExerciseResults = null;
                }
            }
        }

        List<IExerciseResult> _ExerciseResults = null;
        [Ignore]
        public List<IExerciseResult> ExerciseResults
        {
            get
            {
                if (_ExerciseResults == null)
                {
                    // there is no cached version
                    return (_ExerciseResults = Database.ParseIDString<ExerciseResult>(_ExerciseResultsIDs).ToList<IExerciseResult>());
                }
                else
                {
                    // there is a cached version of this object
                    return _ExerciseResults;
                }
            }
            set
            {
                _ExerciseResults = value;
            }
        }

        string _ExerciseIDs = null;
        [Ignore]
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
        IExercise[] Exercises
        {
            get
            {
                // must get Exercises from Routine if it has not been determined yet (_ExerciseIDs == null)
                if (ExerciseIDs == null)
                {
                    //ExerciseIDs = String.Join(",", Routine.Exercises.Select(e => e.ID.ToString()));

                    ExerciseIDs = Routine.Exercises.ToIDString();
                }

                return Database.ParseIDString<Exercise>(ExerciseIDs).ToArray();
            }
        }

        [Ignore]
        public bool Completed
        {
            get
            {
                // idk why ExerciseResults.Count would ever be
                // GREATER than the exercise length, but just in case
                int erc = ExerciseResults.Count;
                int ec = Exercises.Length;
                return erc >= ec;
            }
        }

        public bool Deleted { get; set; }

        public RoutineResult(IRoutine Routine, IPOLDatabase database = null)
            : this(Routine.ID, database)
        {

        }

        public RoutineResult(int RoutineID, IPOLDatabase database = null)
        {
            this.RoutineID = RoutineID;
            //Exercises = Routine.Exercises.ToArray();
            //ExerciseResults = new List<IExerciseResult>();
            this.Database = database;
        }

        public RoutineResult()
        {
            //ExerciseResults = new List<IExerciseResult>();
        }

        [Ignore]
        public IExercise NextExercise
        {
            get
            {
                if (Completed)
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

        public void ReportExerciseResult(IExerciseResult ex_result)
        {
            if (Completed)
            {
                throw new InvalidOperationException("Too many exercise results reported");
            }

            if (!NextExercise.Equals(ex_result.Exercise))
            {
                throw new ArgumentException("ExerciseResult is not for the correct exercise. " +
                    "Use NextExercise to retrieve the expected exercise.");
            }

            if (ResultCount == 0) StartTime = ex_result.Time;

            EndTime = ex_result.Time;

            ExerciseResults.Add(ex_result);

            IExercise exercise = ex_result.Exercise;
            if(exercise.RefreshUsage())
            {
                Database.Update((Exercise)exercise);
            }

            ExerciseDifficulty diff = exercise.GetDifficultyRecord();
            if(diff != null)
            {
                if (diff.RefreshUsage())
                {
                    Database.Update(diff);
                }
            }
        }

        public static IRoutineResult MostRecentForRoutine(IPOLDatabase database, IRoutine r)
        {
            try
            {
                return database.Table<RoutineResult>()
                    .Where(rr => rr.RoutineID == r.ID)
                    .MaxObject(rr => rr.EndTime);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static IRoutineResult MostRecentUncompleted(IPOLDatabase database, IRoutine r)
        {
            IRoutineResult rr = MostRecentForRoutine(database, r);
            if (rr == null) return null;
            if (rr.Completed) return null;
            return rr;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            string td = TimeDetails;
            if (td != null)
            {
                builder.Append(td);
            }

            builder.AppendLine($"{Routine.Name}");

            builder.Append(ShortDetails);

            return builder.ToString();
        }

        public string TimeDetails
        {
            get
            {
                if (this.EndTime != null)
                {
                    TimeSpan span = (this.EndTime - this.StartTime);
                    return $"{this.StartTime} ({(int)span.TotalMinutes} mins) ";
                }
                return null;
            }
        }

        public string ShortDetails
        {
            get
            {
                TimeSpan time_since_last_exr = (DateTime.Now - this.EndTime);

                // for some reason this.EndTime is DateTime.MinValue sometimes
                bool IsRecent =
                    (this.EndTime == null ||
                    time_since_last_exr < TimeSpan.FromDays(1) ||
                    this.EndTime == DateTime.MinValue);

                System.Diagnostics.Debug.WriteLine($"time_since_last_exr={time_since_last_exr}");
                System.Diagnostics.Debug.WriteLine($"this.EndTime={this.EndTime}");
                System.Diagnostics.Debug.WriteLine($"IsRecent={IsRecent}");

                StringBuilder builder = new StringBuilder();
                IExercise last_exercise = null;
                IExercise[] exercises = this.Exercises;
                for(int i = 0; i < exercises.Length; i++)
                {
                    bool first_set_of_exercise = false;
                    if(exercises[i].Equals(last_exercise))
                    {
                        builder.Append(", ");
                    }
                    else
                    {
                        if(last_exercise != null)
                        {
                            builder.AppendLine();
                        }
                        builder.Append(exercises[i].Name + " ");
                        first_set_of_exercise = true;
                    }

                    if(i < ResultCount)
                    {
                        IExerciseResult exr = ExerciseResults[i];
                        builder.Append($"{exr.Weight}x{exr.RepCount}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"first_set_of_exercise={first_set_of_exercise}");
                        if (first_set_of_exercise && IsRecent)
                        {
                            builder.Append(exercises[i].NextWeight + "x__");
                        }
                        else
                        {
                            builder.Append("___");
                        }
                    }
                    

                    last_exercise = exercises[i];
                }




                /*foreach (IExerciseResult exr in this.ExerciseResults)
                {
                    IExercise this_exercise = exr.Exercise;
                    if (!this_exercise.Equals(last_exercise))
                    {
                        if (last_exercise != null)
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
                }*/

                return builder.ToString();
            }
        }



        public IRoutineResult Transform(IRoutine new_routine)
        {
            // 

            if (Routine == new_routine) return this;

            RoutineResult new_rr = new RoutineResult(new_routine, Database);

            int same_count = 0;
            int old_exercise_count = Exercises.Length;
            int new_exercise_count = new_rr.Exercises.Length;
            int max_ex_len = Math.Min(old_exercise_count, new_exercise_count);
            for (int i = 0; i < max_ex_len; i++)
            {
                if (Exercises[i].Equals(new_rr.Exercises[i]))
                {
                    same_count++;
                }
                else
                {
                    break;
                }
            }

            if (same_count < ResultCount)
            {
                throw new InvalidOperationException(
                    "This routine result cannot be transformed into new_routine");
            }

            foreach (ExerciseResult er in ExerciseResults.Take(same_count))
            {
                new_rr.ReportExerciseResult(er);
            }

            return new_rr;
        }


        public static Dictionary<int, int> Import(IEnumerable<RoutineResult> routine_results,
            IPOLDatabase destination, Dictionary<int, int> RoutineLookup, Dictionary<int, int> ExerciseResultLookup)
        {
            // loop through routine results
            // swap the Routine ID, and ExerciseResult IDs for the existing db

            Dictionary<int, int> RoutineResultLookup = new Dictionary<int, int>();

            foreach (RoutineResult routine_result in routine_results)
            {
                routine_result.ExerciseResultsIDs = Helpers.TranslateIDString(
                    routine_result.ExerciseResultsIDs, ExerciseResultLookup);
                routine_result.RoutineID = RoutineLookup[routine_result.RoutineID];
                routine_result.ID = 0;

                destination.InsertOrUpdateNoID(routine_result);
            }

            return RoutineResultLookup;
        }

        public static int TranslateRoutineIDs(IPOLDatabase dab,
            Dictionary<int, int> RoutineLookup)
        {
            int count = 0;
            foreach(RoutineResult rr in dab.Table<RoutineResult>())
            {
                if(RoutineLookup.ContainsKey(rr.RoutineID))
                {
                    rr.RoutineID = RoutineLookup[rr.RoutineID];
                    dab.Update((RoutineResult)rr);
                    count++;
                }
            }

            return count;
        }
    }
}