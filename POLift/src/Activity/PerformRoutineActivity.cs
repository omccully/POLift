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
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            /*if (IsTaskRoot)
            {
                // Started from a Notification and the app is not running, restart the app with back stack
                // Here create a launch Intent which includes the back stack
                Intent intent = new Intent(this, this.GetType());
                // Copy extras from incoming Intent
                intent.PutExtras(this.Intent);
                // Now launch this activity again and immediately return
                TaskStackBuilder.Create(this)
                    .AddNextIntentWithParentStack(intent)
                    .StartActivities();
                return;
            }*/

            NextWarmupView.Visibility = ViewStates.Gone;

            ModifyRestOfRoutineButton.Click += ModifyRestOfRoutineButton_Click;

            int routine_id = Intent.GetIntExtra("routine_id", -1);
            Routine = POLDatabase.ReadByID<Routine>(routine_id);
            RoutineResult recent_uncompleted = RoutineResult.MostRecentUncompleted(Routine);

            // do not prompt for warmup if:
            // rrrid >= 0 or user clicked resume

            int resume_routine_result_id;
            int intent_resume_routine_result_id = Intent.GetIntExtra("resume_routine_result_id", -1);

            if (savedInstanceState == null)
            {
                // if there was no saved state, try to use the RR ID from the intent
                resume_routine_result_id = intent_resume_routine_result_id;
            }
            else
            {
                // prefer to use the saved state, but if there is no RR ID, use the intent's
                resume_routine_result_id = savedInstanceState.GetInt("resume_routine_result_id", intent_resume_routine_result_id);
            }

            if (resume_routine_result_id == 0)
            {
                // a routine result was started but has no contents
                RoutineResult = new RoutineResult(Routine);
            }
            else if (recent_uncompleted != null && recent_uncompleted.ID == resume_routine_result_id)
            {
                // restore saved state
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
                    PromptUserForWarmupRoutine();
                }
                else
                {
                    // TODO: set default value for RoutineResult here?
                    // to prevent null

                    // there is a uncompleted routine result within the last 1 day for this routine
                    // so ask user if they want to resume it

                    Helpers.DisplayConfirmation(this, "You did not finish this routine on " +
                        recent_uncompleted.EndTime.ToString() + ". Would you like to resume it?",
                        delegate
                        {
                            // yes
                            RoutineResult = recent_uncompleted;
                            //SyncTimerBasedOnLastExerciseResult();
                        },
                        delegate
                        {
                            // no
                            RoutineResult = new RoutineResult(Routine);
                            PromptUserForWarmupRoutine();
                        }
                    );
                }
            }

            ActionBar.Title = $"Perform {Routine.Name} routine";

            RefreshGUI();
        }

        const int ModifyRestOfRoutineResultCode = 6000;

        private void ModifyRestOfRoutineButton_Click(object sender, EventArgs e)
        {
            Intent result_intent = new Intent(this, typeof(CreateRoutineActivity));
            result_intent.PutExtra("edit_routine_id", Routine.ID);
            result_intent.PutExtra("exercises_locked", RoutineResult.ResultCount);

            StartActivityForResult(result_intent, ModifyRestOfRoutineResultCode);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if(RoutineResult != null)
            {
                outState.PutInt("resume_routine_result_id", RoutineResult.ID);
            }
            
            base.OnSaveInstanceState(outState);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok)
            {
                if(requestCode == WarmUpRoutineRequestCode)
                {
                    if (CurrentExercise != null)
                    {
                        // start timer once warmup routine is finished.
                        StartRestPeriod();
                    }
                }
                else if(requestCode == ModifyRestOfRoutineResultCode)
                {
                    int id = data.GetIntExtra("routine_id", -1);
                    if (id == -1) return;
                    Routine new_routine = POLDatabase.ReadByID<Routine>(id);

                    if (Routine == new_routine) return;

                    RoutineResult old_rr = RoutineResult;
                    RoutineResult = RoutineResult.Transform(new_routine);

                    POLDatabase.HideDeletable(old_rr);

                    // CreateRoutineActivity removes old routine
                    Routine = new_routine; 

                    GetNextExerciseAndWeight();
                }
            }
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

        void RefreshRoutineDetails()
        {
            if (RoutineResult != null)
            {
                RoutineDetails.Text = RoutineResult.ShortDetails;
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
                    + CurrentExercise.ShortDetails;

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

        protected override void SaveStateToIntent(Intent intent)
        {
            base.SaveStateToIntent(intent);

            intent.PutExtra("routine_id", Routine.ID);

            int rr_id = (RoutineResult == null ? 0 : RoutineResult.ID);

            intent.PutExtra("resume_routine_result_id", rr_id);
        }
    }
}