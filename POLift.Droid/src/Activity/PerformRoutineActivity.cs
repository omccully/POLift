﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Util;

using System.Diagnostics;

using Microsoft.Practices.Unity;

namespace POLift.Droid
{
    using Service;
    using Core.Service;
    using Core.Model;

    [Activity(Label = "Perform routine", ParentActivity = typeof(MainActivity))]
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

                //if (_routine_result != null)
                //{
                //    Intent.PutExtra("resume_routine_result_id", _routine_result.ID);
                //}

                GetNextExerciseAndWeight();
            }
        }

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            System.Diagnostics.Debug.WriteLine("PerformRoutineActivity.OnCreate()");

            

            base.OnCreate(savedInstanceState);
            System.Diagnostics.Debug.WriteLine("base finish " + sw.ElapsedMilliseconds + "ms");
            Log.Debug("POLift", "base finish " + sw.ElapsedMilliseconds + "ms");
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
                Log.Debug("POLift", "Starting PerformRoutine from intent. intent[routine_id] = " +
                    routine_id);
            }
            else
            {
                int routine_id_from_intent = Intent.GetIntExtra("routine_id", -1);
                routine_id = savedInstanceState.GetInt("routine_id", routine_id_from_intent);
                Log.Debug("POLift", "Restoring PerformRoutine from saved state. state[routine_id] = " +
                    savedInstanceState.GetInt("routine_id", -9999) + ", using " + routine_id);
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

                Log.Debug("POLift", "Starting PerformRoutine from intent. intent[resume_routine_result_id] = " +
                    intent_resume_routine_result_id);

            }
            else
            {
                // prefer to use the saved state, but if there is no RR ID, use the intent's
                resume_routine_result_id = savedInstanceState.GetInt("resume_routine_result_id", intent_resume_routine_result_id);

                Log.Debug("POLift", 
                    "Restoring PerformRoutine from saved state. state[resume_routine_result_id] = " +
                    savedInstanceState.GetInt("resume_routine_result_id", -9999) + 
                    ", using " + resume_routine_result_id);
            }

            bool intent_warmup_prompted = Intent.GetBooleanExtra("intent_was_started", false);
            if(savedInstanceState == null)
            {
                Log.Debug("POLift", "Starting PerformRoutine from intent. intent[intent_was_started] = " +
                   intent_warmup_prompted);
                WarmupPrompted = intent_warmup_prompted;
            }
            else
            {
                WarmupPrompted = savedInstanceState.GetBoolean("warmup_prompted",
                    intent_warmup_prompted);

                Log.Debug("POLift",
                    "Restoring PerformRoutine from saved state. state[warmup_prompted] = " +
                    savedInstanceState.GetBoolean("warmup_prompted", false) +
                    ", using " + WarmupPrompted);
            }


            /* Intent.GetBooleanExtra("backed_into", false) ||  */
            Log.Debug("POLift", "perform_z finish " + sw.ElapsedMilliseconds + "ms");
            if (resume_routine_result_id == 0)
            {
                Log.Debug("POLift", "No resume_routine_result_id, starting new RoutineResult");

                // a routine result was started but has no contents
                _RoutineResult = new RoutineResult(Routine);
            }
            else if (recent_uncompleted != null && recent_uncompleted.ID == resume_routine_result_id)
            {
                Log.Debug("POLift", "Restoring RoutineResult with ID " + resume_routine_result_id);

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
                    Log.Debug("POLift", "Starting new RoutineResult and prompting for warmup routine");

                    _RoutineResult = new RoutineResult(Routine);
                    if(!WarmupPrompted)
                    {
                        PromptUserForWarmupRoutine();
                    }
                }
                else
                {
                    // there is a uncompleted routine result within the last 1 day for this routine
                    // so ask user if they want to resume it
                    Log.Debug("POLift", "Prompting user to resume an uncompleted routine");

                    PromptUserToResumeRoutine(recent_uncompleted);
                }
            }

            // var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);
            //ActionBar.Title = $"Perform {Routine.Name} routine";
            Log.Debug("POLift", "perform_0 finish " + sw.ElapsedMilliseconds + "ms");
            RefreshGUI();

            Log.Debug("POLift", "perform finish " + sw.ElapsedMilliseconds + "ms");
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

            outState.PutBoolean("warmup_prompted", WarmupPrompted);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            System.Diagnostics.Debug.WriteLine("PerformRoutineActivity.OnActivityResult()");
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

                    if (Routine.Equals(new_routine)) return;

                    Intent.PutExtra("routine_id", new_routine.ID);

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
            float weight = 0;
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

        bool ReportExerciseResult(float weight, int reps)
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

            PromptUserForRating();
           
            return true;
        }

        void PromptUserForRating()
        {
            const string ask_for_rating_pref_key = "ask_for_rating";

            if (_RoutineResult.ResultCount == 1)
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
                bool ask_for_rating = prefs.GetBoolean(ask_for_rating_pref_key, true);
                if (ask_for_rating)
                {
                    int rr_count = Database.Table<RoutineResult>().Count();
                    Log.Debug("POLift", "rr_count = " + rr_count);
                    if (rr_count == 10 || rr_count > 15)
                    {
                        AndroidHelpers.DisplayConfirmationYesNotNowNever(this,
                            "Thank you for using POLift. Would you like to " +
                            "rate this app in the Google Play store? ",
                            ask_for_rating_pref_key, delegate
                            {
                                try
                                {
                                    StartActivity(new Intent(Intent.ActionView,
                                        Android.Net.Uri.Parse("market://details?id=com.cml.polift")));
                                    prefs.Edit().PutBoolean("has_rated_app", true).Apply();
                                }
                                catch { }
                            });

                        if (rr_count > 15)
                        {
                            prefs.Edit().PutBoolean(ask_for_rating_pref_key, false).Apply();
                        }
                    }
                }
            }
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

        void RefreshGUI()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            RefreshRoutineDetails();
            Log.Debug("POLift", "RefreshRoutineDetails " + sw.ElapsedMilliseconds + "ms");
            RefreshExerciseDetails();
            Log.Debug("POLift", "RefreshExerciseDetails+ " + sw.ElapsedMilliseconds + "ms");
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

            RefreshPreviousRepCounts();
        }

        async Task RefreshPreviousRepCounts()
        {
            string new_text = "";

            //IEnumerable<ExerciseResult> previous_ers = Database.Table<ExerciseResult>()
            //   .Where(er => er.ExerciseID == CurrentExercise?.ID &&
            //       er.Time < _RoutineResult.StartTime)
            //        .TakeLastEx(3);

            DateTime routine_start_time = _RoutineResult.StartTime == default(DateTime) ?
                DateTime.MaxValue : _RoutineResult.StartTime;
            IEnumerable<ExerciseResult> previous_ers =
                Database.Query<ExerciseResult>(
                    "SELECT * FROM ExerciseResult WHERE ExerciseID = ? AND Time < ? ORDER BY Time DESC LIMIT 3",
                    CurrentExercise?.ID, routine_start_time).OrderBy(er => er.Time);


            //.Where(er => er.ExerciseID == CurrentExercise?.ID &&
            //    er.Time < _RoutineResult.StartTime)
            // .TakeLast(3);
            if (previous_ers.Count() == 0)
            {
                Log.Debug("POLift", "no previous exercise results found");
                new_text = "";
            }
            else
            {
                ExerciseResult first = previous_ers.First();
                ExerciseResult previous = first;
                StringBuilder sb = new StringBuilder($" (prev: {first.Weight}x{first.RepCount}");

                foreach (ExerciseResult er in previous_ers.Skip(1))
                {
                    if (er.Weight == previous.Weight)
                    {
                        sb.Append($", x{er.RepCount}");
                    }
                    else
                    {
                        sb.Append($", {er.Weight}x{er.RepCount}");
                    }


                    previous = er;
                }
                sb.Append(")");

                new_text = sb.ToString();
            }

            this.RunOnUiThread(delegate
            {
                RepDetailsTextView.Text = new_text;
            });
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
            if (WarmupPrompted)
            {
                // this method should only be used once
                Log.Debug("POLift", "Prevented additional prompt for warmup routine");

                return; 
            }
            WarmupPrompted = true;

            string exercise_name;
            if (CurrentExercise == null)
            {
                exercise_name = "";
            }
            else
            {
                string en = CurrentExercise.Name.ToLower();

                bool is_vowel = "aeiouAEIOU".IndexOf(en[0]) >= 0;

                exercise_name = (is_vowel ? "n " : " ") + en;
            }

            AndroidHelpers.DisplayConfirmationNeverShowAgain(this,
                $"Would you like to do a{exercise_name} warmup routine?",
                "warmup", delegate
                {
                    StartWarmupActivity();
                });
        }

        void DefaultWarmupTo(ISharedPreferences prefs, bool default_warmup)
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean(AskForWarmupPreferenceKey, false);
            editor.PutBoolean(DefaultWarmupPreferenceKey, default_warmup);
            editor.Apply();
        }


        bool WarmupPrompted = false;
        void StartWarmupActivity()
        {
            var intent = new Intent(this, typeof(WarmupRoutineActivity));
            intent.PutExtra("exercise_id", CurrentExercise.ID);
            intent.PutExtra("working_set_weight", WeightInput);
            intent.PutExtra("parent_intent", this.Intent);
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

            intent.PutExtra("warmup_prompted", WarmupPrompted);
        }

        protected override void OnPause()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnPause()");

            base.OnPause();
        }

        protected override void OnStop()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnStop()");

            base.OnStop();
        }

        protected override void OnDestroy()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnDestroy()");

            base.OnDestroy();
        }
    }
}
 