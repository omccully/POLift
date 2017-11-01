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
using Android.Preferences;

using Microsoft.Practices.Unity;

namespace POLift
{
    using Service;
    using Model;

    [Activity(Label = "Perform routine")]
    public class PerformRoutineActivity : PerformRoutineBaseActivity
    {
        const int WarmUpRoutineRequestCode = 100;
        const int EditRoutineResultRequestCode = 991234;

        IRoutine Routine;

        AlertDialog dialog;

        IRoutineResult _routine_result;
        IRoutineResult _RoutineResult
        {
            get
            {
                return _routine_result;
            }
            set
            {
                _routine_result = value;
                _routine_result.Database = Database;
                GetNextExerciseAndWeight();
            }
        }

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Database = C.ontainer.Resolve<IPOLDatabase>();

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

            IMadeAMistakeButton.Click += IMadeAMistakeButton_Click;

            NextWarmupView.Visibility = ViewStates.Gone;

            ModifyRestOfRoutineButton.Click += ModifyRestOfRoutineButton_Click;

            int routine_id;
            if(savedInstanceState == null)
            {
                routine_id = Intent.GetIntExtra("routine_id", -1);
            }
            else
            {
                routine_id = savedInstanceState.GetInt("routine_id", Intent.GetIntExtra("routine_id", -1));
            }
            
            
            Routine = Database.ReadByID<Routine>(routine_id);
            IRoutineResult recent_uncompleted = RoutineResult.MostRecentUncompleted(Database, Routine);

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
                _RoutineResult = new RoutineResult(Routine);
            }
            else if (recent_uncompleted != null && recent_uncompleted.ID == resume_routine_result_id)
            {
                // restore saved state
                _RoutineResult = recent_uncompleted;
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

                    _RoutineResult = new RoutineResult(Routine);
                    PromptUserForWarmupRoutine();
                }
                else
                {
                    // there is a uncompleted routine result within the last 1 day for this routine
                    // so ask user if they want to resume it
                
                    PromptUserToResumeRoutine(recent_uncompleted);
                }
            }

           // var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);
            //ActionBar.Title = $"Perform {Routine.Name} routine";

            RefreshGUI();
        }

        

        private void IMadeAMistakeButton_Click(object sender, EventArgs e)
        {
            if (_RoutineResult == null)
            {
                Toast.MakeText(this, "Error :/",
                    ToastLength.Long).Show();
                return;
            }
            if (_RoutineResult.ResultCount == 0)
            {
                Toast.MakeText(this, "You have not started this routine yet",
                    ToastLength.Long).Show();
                return;
            }

            Intent intent = new Intent(this, typeof(EditRoutineResultActivity));
            intent.PutExtra("routine_result_id", _RoutineResult.ID);
            StartActivityForResult(intent, EditRoutineResultRequestCode);
        }

        const int ModifyRestOfRoutineResultCode = 6000;

        private void ModifyRestOfRoutineButton_Click(object sender, EventArgs e)
        {
            Intent result_intent = new Intent(this, typeof(CreateRoutineActivity));
            result_intent.PutExtra("edit_routine_id", Routine.ID);
            result_intent.PutExtra("exercises_locked", _RoutineResult.ResultCount);

            StartActivityForResult(result_intent, ModifyRestOfRoutineResultCode);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if(_RoutineResult != null)
            {
                outState.PutInt("resume_routine_result_id", _RoutineResult.ID);
            }

            outState.PutInt("routine_id", Routine.ID);
            
            base.OnSaveInstanceState(outState);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok)
            {
                if (requestCode == WarmUpRoutineRequestCode)
                {
                    if (CurrentExercise != null)
                    {
                        // start timer once warmup routine is finished.
                        StartRestPeriod();
                    }
                }
                else if (requestCode == ModifyRestOfRoutineResultCode)
                {
                    int id = data.GetIntExtra("routine_id", -1);
                    if (id == -1) return;
                    Routine new_routine = Database.ReadByID<Routine>(id);

                    if (Routine == new_routine) return;

                    if (_RoutineResult.ResultCount == 0)
                    {
                        // routine wasn't started
                        _RoutineResult = new RoutineResult(new_routine, Database);
                    }
                    else
                    {
                        IRoutineResult old_rr = _RoutineResult;
                        _RoutineResult = _RoutineResult.Transform(new_routine);

                        Database.HideDeletable((RoutineResult)old_rr);

                        Database.Insert((RoutineResult)_RoutineResult);
                    }

                    // CreateRoutineActivity removes old routine
                    Routine = new_routine;

                    GetNextExerciseAndWeight();
                }
                else if(requestCode == EditRoutineResultRequestCode)
                {
                    RefreshRoutineDetails();
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
                Toast.MakeText(this,
                        "You must fill out the weight and rep count with integers",
                    ToastLength.Long).Show();
                return;
            }

            RepResultEditText.Text = "";

            if(ReportExerciseResult(weight, reps))
            {
                // if there's more exercises, try to show an ad
                TryShowFullScreenAd();
            }
        }

        bool ReportExerciseResult(int weight, int reps)
        {
            // report the exercise result
            ExerciseResult ex_result =
                new ExerciseResult(CurrentExercise, weight, reps);
            ex_result.Database = Database;
            Database.Insert(ex_result);
            _RoutineResult.ReportExerciseResult(ex_result);

            // insert or update the routine result after EVERY new result
            // just in case the app crashes or something
            Database.InsertOrUpdateByID(_RoutineResult);


            // TODO: make this say shit like: "nice! You've met your rep goal
            // for ___ sets in a row. You just need 1 more!"

            // TODO: break up NextWeight into a method that gives you
            // how many sets in a row you've succeeded at. 
            // 

            //if(reps >= CurrentExercise.MaxRepCount)
            if(CurrentExercise.NextWeight > weight)
            {
                Toast.MakeText(this, "Weight increase!", ToastLength.Long).Show();
            } 
            else 
            {
                
                int needed_succeeds_in_a_row = CurrentExercise.ConsecutiveSetsForWeightIncrease;
                if(needed_succeeds_in_a_row > 1)
                {
                    int succeeds_in_a_row = CurrentExercise.SucceedsInARow();
                    string plur = succeeds_in_a_row > 1 ? "s" : "";

                    int needed_left = needed_succeeds_in_a_row - succeeds_in_a_row;

                    Toast.MakeText(this, "Nice! You met your rep goal for " +
                        $"{succeeds_in_a_row} set{plur} in a row. " + 
                        $"You need {needed_left} more in a row to advance to the next weight", 
                        ToastLength.Long).Show();
                }

            }

            if (_RoutineResult.Completed)
            {
                // no more exercises
                ReturnRoutineResult(_RoutineResult);
                StaticTimer.StopTimer();
                CancelTimerNotification();
                return false;
            }

            // update Weight and CurrentExercise
            GetNextExerciseAndWeight();
            // rest period is based on the NEXT exercise's rest period

            StartRestPeriod();
            return true;
        }

        void StartRestPeriod()
        {
            // execute rest period, disable button and text box
            StartTimer(CurrentExercise.RestPeriodSeconds);
        }

        void RefreshRoutineDetails()
        {
            if (_RoutineResult != null)
            {
                RoutineDetails.Text = _RoutineResult.ShortDetails;
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
                NextExerciseView.Text = $"Exercise {_RoutineResult.ResultCount+1}/"
                    + $"{ _RoutineResult.ExerciseCount}: "
                    + CurrentExercise.ShortDetails;

            }
            else if (_RoutineResult != null && _RoutineResult.Completed)
            {
                NextExerciseView.Text = "Routine completed" + (_RoutineResult == null ? "" : "!");
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
            CurrentExercise = _RoutineResult.NextExercise;

            if (CurrentExercise != null)
            {
                WeightInput = CurrentExercise.NextWeight;
            }

            RefreshGUI();
        }

        void SyncTimerBasedOnLastExerciseResult()
        {
            if (_RoutineResult == null) return;

            IExerciseResult last_ex_result = _RoutineResult.ExerciseResults.LastOrDefault();
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

        const string AskForRoutineResumePreferenceKey = "ask_for_routine_resume";
        void PromptUserToResumeRoutine(IRoutineResult recent_uncompleted)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            bool AskForRoutineResume = 
                prefs.GetBoolean(AskForRoutineResumePreferenceKey, true);

            if (AskForRoutineResume)
            {
                _RoutineResult = new RoutineResult(Routine);

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage("You did not finish this routine on " +
                    recent_uncompleted.EndTime.ToString() + ". Would you like to resume it?");
                builder.SetNeutralButton("Yes", delegate
                {
                    // yes
                    _RoutineResult = recent_uncompleted;
                    //SyncTimerBasedOnLastExerciseResult();
                });

                builder.SetNegativeButton("No", delegate
                {
                    // no
                    _RoutineResult = new RoutineResult(Routine);
                    PromptUserForWarmupRoutine();
                });

                builder.SetPositiveButton("Yes, never show again", delegate
                {
                    _RoutineResult = recent_uncompleted;

                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutBoolean(AskForRoutineResumePreferenceKey, false);
                    editor.Apply();
                });

              
                AlertDialog ad = builder.Create();
                ad.Show();
                builder.Dispose();
                ad.Show();

            }
            else
            {
                // if don't ask, then assume yes
                _RoutineResult = recent_uncompleted;
            }
            
        }


        const string AskForWarmupPreferenceKey = "ask_for_warmup";
        const string DefaultWarmupPreferenceKey = "default_warmup";
        void PromptUserForWarmupRoutine()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            
            bool AskForWarmup = prefs.GetBoolean(AskForWarmupPreferenceKey, true);
            bool DefaultWarmup = prefs.GetBoolean(DefaultWarmupPreferenceKey, false);

            if (AskForWarmup)
            {
                string exercise_name;
                if(CurrentExercise == null)
                {
                    exercise_name = "";
                }
                else
                {
                    string en = CurrentExercise.Name.ToLower();

                    bool is_vowel = "aeiouAEIOU".IndexOf(en[0]) >= 0;

                    exercise_name = (is_vowel ? "n " : " ") + en;
                }

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage($"Would you like to do a{exercise_name} warmup routine?");
                
                View CheckBoxLayout = LayoutInflater.Inflate(Resource.Layout.NeverShowAgainCheckBox, null);
                CheckBox NeverShowAgainCheckBox = CheckBoxLayout.FindViewById<CheckBox>(
                    Resource.Id.NeverShowAgainCheckBox);

                builder.SetView(CheckBoxLayout);

                builder.SetPositiveButton("Yes", delegate
                {
                    System.Diagnostics.Debug.WriteLine("yes: " + NeverShowAgainCheckBox.Checked);
                    if (NeverShowAgainCheckBox.Checked)
                    {
                        DefaultWarmupTo(prefs, true);
                    }

                    StartWarmupActivity();
                });

                builder.SetNegativeButton("No", delegate 
                {
                    System.Diagnostics.Debug.WriteLine("no: " + NeverShowAgainCheckBox.Checked);
                    if (NeverShowAgainCheckBox.Checked)
                    {
                        DefaultWarmupTo(prefs, false);
                    }
                });

                dialog = builder.Create();
                builder.Dispose();
                dialog.Show();
            }
            else
            {
                if(DefaultWarmup)
                {
                    StartWarmupActivity();
                }
            }
        }

        void DefaultWarmupTo(ISharedPreferences prefs, bool default_warmup)
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean(AskForWarmupPreferenceKey, false);
            editor.PutBoolean(DefaultWarmupPreferenceKey, default_warmup);
            editor.Apply();
        }

        void StartWarmupActivity()
        {
            var intent = new Intent(this, typeof(WarmupRoutineActivity));
            intent.PutExtra("exercise_id", CurrentExercise.ID);
            intent.PutExtra("working_set_weight", WeightInput);
            StartActivityForResult(intent, WarmUpRoutineRequestCode);
        }

        void ReturnRoutineResult(IRoutineResult routine_result)
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

            int rr_id = (_RoutineResult == null ? 0 : _RoutineResult.ID);

            intent.PutExtra("resume_routine_result_id", rr_id);
        }
    }
}