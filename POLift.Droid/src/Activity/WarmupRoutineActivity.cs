﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Microsoft.Practices.Unity;

using System.Diagnostics;

namespace POLift.Droid
{
    using Service;
    using Android.Text;
    using Core.Model;
    using Core.Service;

    [Activity(Label = "Warmup", ParentActivity = typeof(PerformRoutineActivity))]
    public class WarmupRoutineActivity : PerformRoutineBaseActivity
    {
        IExercise FirstExercise = null;

        const string WarmupSetIndexKey = "warmup_set_index";

        IWarmupSet[] WarmupSets = WarmupSet.Default;

        IWarmupSet NextWarmupSet
        {
            get
            {
                if (WarmupFinished) return null;
                return WarmupSets[WarmupSetIndex];
            }
        }

        int _warmup_set_index = 0;
        int WarmupSetIndex
        {
            get
            {
                return _warmup_set_index;
            }
            set
            {
                _warmup_set_index = value;
                RefreshWarmupInfo();
            }
        }

        bool WarmupFinished
        {
            get
            {
                return WarmupSetIndex >= WarmupSets.Length;
            }
        }

        Dialog error_dialog;
        Dialog back_button_dialog;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Log.Debug("POLift", "WarmupRoutineActivity.OnCreate()");

            base.OnCreate(savedInstanceState);
            Log.Debug("POLift", "Warmup after base " + sw.ElapsedMilliseconds + "ms");


            // Create your application here

            Database = C.ontainer.Resolve<IPOLDatabase>();

            int id = Intent.GetIntExtra("exercise_id", -1);
            if (id != -1)
            {
                FirstExercise = Database.ReadByID<Exercise>(id);
            }

            WeightInput = Intent.GetFloatExtra("working_set_weight", 0);

            if (FirstExercise == null)
            {
                error_dialog = AndroidHelpers.DisplayError(this, "Error (" + id + ")",
                    delegate
                    {
                        Finish();
                    });

                return;
            }

            int warmup_set_index_intent = Intent.GetIntExtra("warmup_set_index", 0);
            if (savedInstanceState == null)
            {
                Log.Debug("POLift", "Starting WarmupRoutine from intent. intent[warmup_set_index] = " +
                    warmup_set_index_intent);
                WarmupSetIndex = warmup_set_index_intent;
            }
            else
            {
                Log.Debug("POLift", "Restoring WarmupRoutine from saved state. state[warmup_set_index] = " +
                    savedInstanceState.GetInt("warmup_set_index", -9999));
                WarmupSetIndex = savedInstanceState.GetInt("warmup_set_index", warmup_set_index_intent);
            }

            // set index and display the stuff

            //RoutineDetails.Visibility = ViewStates.Gone;
            WeightLabel.Text = "Working set weight: ";
            ReportResultButton.Text = "Set completed";
            RepResultLabel.Visibility = ViewStates.Gone;
            RepResultEditText.Visibility = ViewStates.Gone;
            ModifyRestOfRoutineButton.Visibility = ViewStates.Gone;
            NextExerciseView.Visibility = ViewStates.Gone;

            //IMadeAMistakeButton.Visibility = ViewStates.Gone;
            IMadeAMistakeButton.Text = "Skip warmup routine";
            IMadeAMistakeButton.Click += IMadeAMistakeButton_Click;

            //var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);
            //ActionBar.Title = $"{FirstExercise.Name} warmup";

            RepDetailsTextView.Visibility = ViewStates.Gone;

            Log.Debug("POLift", "Warmup final " + sw.ElapsedMilliseconds + "ms");

        }



        private void IMadeAMistakeButton_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            Finish();
        }

        public override void OnBackPressed()
        {
            if(WarmupSetIndex == 0)
            {
                base.OnBackPressed();
                return;
            }

            back_button_dialog = AndroidHelpers.DisplayConfirmation(this,
                "Are you sure you want to end this warmup session? " +
                " You will lose all of your progress in this warmup.",
                delegate
                {
                    base.OnBackPressed();
                });
        }

        protected override void WeightEditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            // base.WeightEditText_TextChanged(sender, e);

            RefreshWarmupInfo();
        }

        void RefreshWarmupInfo()
        {
            RefreshCurrentWarmupDetails();

            RefreshFullWarmupDetails();
        }

        void RefreshCurrentWarmupDetails()
        {
            if (WarmupFinished)
            {
                NextExerciseView.Text = "Finished";
                return;
            }

            string txt = $"Warmup exercise {WarmupSetIndex + 1}/{WarmupSets.Count()}: ";
            txt += $"{NextWarmupSet.Reps} reps of {FirstExercise.Name} at a weight of ";

            try
            {
                float weight = NextWarmupSet.GetWeight(FirstExercise, WeightInput);
                txt += weight.ToString();

                if (FirstExercise.PlateMath != null)
                {
                    txt += " (" + FirstExercise.PlateMath.PlateCountsToString(weight) + ")";
                }
            }
            catch (FormatException)
            {
                txt += "??";
            }


            if (!String.IsNullOrEmpty(NextWarmupSet.Notes))
            {
                txt += $" ({NextWarmupSet.Notes})";
            }

            NextWarmupView.Text = txt;
        }

        void RefreshFullWarmupDetails()
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                float weight = WeightInput;

                int i = 0;
                foreach (IWarmupSet ws in WarmupSets)
                {
                    if (i == WarmupSetIndex)
                    {
                        builder.Append("> ");
                    }

                    builder.Append("Weight of ");
                    builder.Append(ws.GetWeight(FirstExercise, weight).ToString());
                    builder.Append(", rest ");
                    builder.Append(ws.GetRestPeriod(FirstExercise).ToString());

                    if (i < WarmupSetIndex)
                    {
                        builder.Append(" (done)");
                    }

                    if(i < WarmupSets.Length - 1)
                    {
                        builder.AppendLine();
                    }

                    i++;
                }

                RoutineDetails.Text = builder.ToString();
            }
            catch (FormatException)
            {
                RoutineDetails.Text = "??";
            }
        }

        protected override void ReportResultButton_Click(object sender, EventArgs e)
        {
            // warmup set completed button clicked
            WarmupSetIndex++;

            if (WarmupFinished)
            {
                SurpressTimerCallbackCleanup = true;
                SetResult(Result.Ok);
                Finish();
                return;
            }

            StartTimer(NextWarmupSet.GetRestPeriod(FirstExercise));

            TryShowFullScreenAd();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("warmup_set_index", WarmupSetIndex);

            SaveTimerState(outState);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnPause()");

            // dismiss dialog boxes to prevent window leaks
            error_dialog?.Dismiss();
            back_button_dialog?.Dismiss();

            base.OnPause();
        }

        protected override void OnStop()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnStop()");

            base.OnStop();
        }


        protected override void OnDestroy()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnDestroy()");

            base.OnDestroy();
        }

        protected override void SaveStateToIntent(Intent intent)
        {
            base.SaveStateToIntent(intent);

            intent.PutExtra("exercise_id", FirstExercise.ID);

            try
            {
                intent.PutExtra("working_set_weight", WeightInput);
            }
            catch (FormatException)
            {
            }

            intent.PutExtra("warmup_set_index", WarmupSetIndex);
        }  
    }
}