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

        ExerciseResult[] ExerciseResults { get; set; }
        Exercise[] Exercises { get; set; }
        int next_exercise_index = 0;

        public RoutineResult(Routine Routine) : this(Routine.ID)
        {
            
        }

        public RoutineResult(int RoutineID)
        {
            this.RoutineID = RoutineID;
            
            Exercises = Routine.Exercises.ToArray();
            ExerciseResults = new ExerciseResult[Exercises.Length];
        }

        public RoutineResult() : this(0)
        {

        }

        public Exercise NextExercise
        {
            get
            {
                if(next_exercise_index >= Exercises.Length)
                {
                    return null;
                }

                return Exercises[next_exercise_index];
            }
        }

        public void ReportExerciseResult(ExerciseResult ex_result)
        {
            if (next_exercise_index >= Exercises.Length)
            {
                throw new InvalidOperationException("Too many exercise results reported");
            }

            if (StartTime == null) StartTime = ex_result.Time;
            EndTime = ex_result.Time;

            ExerciseResults[next_exercise_index] = ex_result;
            next_exercise_index++;
        }
    }
}