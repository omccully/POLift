using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift
{
    using Service;
    using Model;

    [Activity(Label = "Perform routine")]
    public class PerformRoutineActivity : PerformRoutineBaseActivity
    {
        const int WarmUpRoutineRequestCode = 100;

        Routine Routine;

        AlertDialog dialog;

        bool AskForWarmupRoutine = true;

        RoutineResult _routine_result;
        RoutineResult RoutineResult
        {
            get
            {
                return _routine_result;
            }
            set
            {
                _routine_result = value;
                GetNextExerciseAndWeight();

                SyncTimerBasedOnLastExerciseResult();

                if (AskForWarmupRoutine)
                {
                    PromptUserForWarmupRoutine();
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            int routine_id = Intent.GetIntExtra("routine_id", -1);
            Routine = POLDatabase.ReadByID<Routine>(routine_id);
            RoutineResult recent_uncompleted = RoutineResult.MostRecentUncompleted(Routine);
            int resume_routine_result_id = (savedInstanceState == null ? -2 :
                    savedInstanceState.GetInt("resume_routine_result_id", -1));

            if (resume_routine_result_id == 0)
            {
                // a routine result was started but has no contents
                AskForWarmupRoutine = false;
                RoutineResult = new RoutineResult(Routine);
            }
            else if (recent_uncompleted != null && recent_uncompleted.ID == resume_routine_result_id)
            {
                // restore saved state
                AskForWarmupRoutine = false;
                RoutineResult = recent_uncompleted;
            }
            else
            {
                // there was no saved state from screen rotating:

                if (recent_uncompleted == null ||
                   (DateTime.Now - recent_uncompleted.EndTime) > TimeSpan.FromDays(1))
                {
                    // if there is no recent uncompleted routine result for this routine 
                    // OR
                    // if the most recent uncompleted routine result is more than a day ago, 
                    // just start a new one without asking

                    RoutineResult = new RoutineResult(Routine);
                }
                else
                {
                    // there is a uncompleted routine result within the last 1 day for this routine
                    // so ask user if they want to resume it

                    Helpers.DisplayConfirmation(this, "You did not fiish this routine on " +
                        recent_uncompleted.EndTime.ToString() + ". Would you like to resume it?",
                        delegate
                        {
                            // yes
                            AskForWarmupRoutine = false;
                            RoutineResult = recent_uncompleted;
                        },
                        delegate
                        {
                            // no
                            RoutineResult = new RoutineResult(Routine);
                        }
                    );
                }
            }

            RefreshGUI();
        }

        

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == WarmUpRoutineRequestCode)
            {
                if(CurrentExercise != null)
                {
                    // start timer once warmup routine is finished.
                    StartRestPeriod();
                }
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if(RoutineResult != null)
            {
                outState.PutInt("resume_routine_result_id", RoutineResult.ID);
            }

            base.OnSaveInstanceState(outState);
        }


        protected override void ReportResultButton_Click(object sender, EventArgs e)
        {
            // user submitted a result for this CurrentExercise

            // get number of reps and clear the text box
            int reps = 0;
            int weight = 0;
            try
            {
                weight = WeightInput;
                reps = Int32.Parse(RepResultEditText.Text);
            }
            catch (FormatException)
            {
                Helpers.DisplayError(this,
                    "You must fill out the weight and rep count with integers");
                return;
            }

            RepResultEditText.Text = "";

            ReportExerciseResult(weight, reps);
        }

        void ReportExerciseResult(int weight, int reps)
        {
            // report the exercise result
            ExerciseResult ex_result =
                new ExerciseResult(CurrentExercise, weight, reps);
            POLDatabase.Insert(ex_result);
            RoutineResult.ReportExerciseResult(ex_result);

            // insert or update the routine result after EVERY new result
            // just in case the app crashes or something
            POLDatabase.InsertOrUpdateByID(RoutineResult);

            if (RoutineResult.Completed)
            {
                // no more exercises
                ReturnRoutineResult(RoutineResult);
                return;
            }

            // update Weight and CurrentExercise
            GetNextExerciseAndWeight();
            // rest period is based on the NEXT exercise's rest period

            StartRestPeriod();
        }

        void StartRestPeriod()
        {
            // execute rest period, disable button and text box
            StartTimer(CurrentExercise.RestPeriodSeconds);
        }


        string ExtendedRoutineDetails(Routine routine)
        {
            /*StringBuilder sb = new StringBuilder();
            sb.Append(routine.Name);
            sb.Append(":");
            sb.Append(System.Environment.NewLine);*/

            return routine.ToString();
        }

        void RefreshRoutineDetails()
        {
            if (RoutineResult != null)
            {
                RoutineDetails.Text = RoutineResult.ToString();
            }
            else if (Routine != null)
            {
                RoutineDetails.Text = Routine.ToString();
            }
            else
            {
                RoutineDetails.Text = "?";
            }
        }

        void RefreshExerciseDetails()
        {
            if (CurrentExercise != null)
            {
                NextExerciseView.Text = $"Exercise {RoutineResult.ResultCount+1}/"
                    + $"{ RoutineResult.ExerciseCount}: "
                    + CurrentExercise.ToString();

            }
            else if (RoutineResult != null && RoutineResult.Completed)
            {
                NextExerciseView.Text = "Routine completed";
            }
            else
            {
                NextExerciseView.Text = "Pending";
            }
        }

        void RefreshGUI()
        {
            RefreshRoutineDetails();
            RefreshExerciseDetails();
        }

        void GetNextExerciseAndWeight()
        {
            // get next exercise
            CurrentExercise = RoutineResult.NextExercise;

            if (CurrentExercise != null)
            {
                WeightInput = CurrentExercise.NextWeight;
            }

            RefreshGUI();
        }

        void SyncTimerBasedOnLastExerciseResult()
        {
            if (RoutineResult == null) return;

            ExerciseResult last_ex_result = RoutineResult.ExerciseResults.LastOrDefault();
            if (last_ex_result == null) return;

            int rest_period_seconds = last_ex_result.Exercise.RestPeriodSeconds;
            TimeSpan rest_period_span = TimeSpan.FromSeconds(rest_period_seconds);
            TimeSpan time_since_last_exercise = (DateTime.Now - last_ex_result.Time);
            if (time_since_last_exercise < rest_period_span)
            {
                int remaining_timer = (int)(rest_period_seconds - time_since_last_exercise.TotalSeconds);
                StartTimer(remaining_timer);
            }
        }

        void PromptUserForWarmupRoutine()
        {
            dialog = Helpers.DisplayConfirmation(this, "Would you like to do a warmup routine?",
                delegate
                {
                    var intent = new Intent(this, typeof(WarmupRoutineActivity));
                    intent.PutExtra("exercise_id", CurrentExercise.ID);
                    intent.PutExtra("working_set_weight", WeightInput);
                    StartActivityForResult(intent, WarmUpRoutineRequestCode);
                });
        }

        void ReturnRoutineResult(RoutineResult routine_result)
        {
            ReturnRoutineResult(routine_result.ID);
        }

        void ReturnRoutineResult(int ID)
        {
            var result_intent = new Intent();
            result_intent.PutExtra("routine_result_id", ID);
            SetResult(Result.Ok, result_intent);
            Finish();
        }
    }
}