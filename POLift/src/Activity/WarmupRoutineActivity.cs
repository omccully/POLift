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

namespace POLift
{
    using Model;
    using Service;

    class WarmupSet
    {
        int PercentOfWeight;
        int PercentOfRestPeriod;

        public readonly int Reps;
        public readonly string Notes;

        public WarmupSet(int reps, int percent_of_weight, int percent_of_rest_period, string notes="")
        {
            Reps = reps;
            PercentOfWeight = percent_of_weight;
            PercentOfRestPeriod = percent_of_rest_period;
            Notes = notes;
        }

        public int GetWeight(Exercise ex, int max_weight)
        {
            return Helpers.GetClosestToIncrement((max_weight * PercentOfWeight) / 100, 
                ex.WeightIncrement,
                max_weight % ex.WeightIncrement);
        }

        public int GetRestPeriod(Exercise ex)
        {
            return (ex.RestPeriodSeconds * PercentOfRestPeriod) / 100;
        }
    }

    [Activity(Label = "Warmup")]
    public class WarmupRoutineActivity : Activity
    {
        Button WarmupSetFinishedButton;
        EditText WorkingSetWeightEditText;
        TextView WarmupRoutineTextView;
        TextView TimeLeftTextView;

        Exercise first_exercise = null;
        int working_weight = 0;

        WarmupSet[] warmup_sets =
        {
            new WarmupSet(8, 50, 50),
            new WarmupSet(8, 50, 50),
            new WarmupSet(4, 70, 50),
            new WarmupSet(1, 90, 50)
        };


        WarmupSet next_warmup_set;

        int _warmup_set_index = 0;
        int warmup_set_index
        {
            get
            {
                return _warmup_set_index;
            }
            set
            {
                _warmup_set_index = value;

                if (value >= warmup_sets.Length)
                {
                    WarmupRoutineTextView.Text = "Finished";
                    return;
                }

                WarmupSet ws = warmup_sets[value];
                string txt = $"{ws.Reps} reps of {first_exercise.Name} at a weight of ";
                txt += ws.GetWeight(first_exercise, working_weight).ToString();
                if (!String.IsNullOrEmpty(ws.Notes))
                {
                    txt += $" ({ws.Notes})";
                }

                WarmupRoutineTextView.Text = txt;
                next_warmup_set = ws;
            }
        }

        bool Finished
        {
            get
            {
                return warmup_set_index >= warmup_sets.Length;
            }
        }

        Dialog error_dialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.WarmupRoutine);

            WarmupSetFinishedButton = FindViewById<Button>(Resource.Id.WarmupSetFinishedButton);
            WorkingSetWeightEditText = FindViewById<EditText>(Resource.Id.WorkingSetWeightEditText);
            WarmupRoutineTextView = FindViewById<TextView>(Resource.Id.WarmupRoutineTextView);
            TimeLeftTextView = FindViewById<TextView>(Resource.Id.TimeLeftTextView);

            int id = Intent.GetIntExtra("exercise_id", -1);
            if (id != -1)
            {
                first_exercise = POLDatabase.ReadByID<Exercise>(id);
            }

            working_weight = Intent.GetIntExtra("working_set_weight", 0);
            WorkingSetWeightEditText.Text = working_weight.ToString();

            if (first_exercise == null)
            {
                error_dialog = Helpers.DisplayError(this, "Error (" + id + ")",
                    delegate
                    {
                        Finish();
                    });

                return;
            }

            WorkingSetWeightEditText.TextChanged += WorkingSetWeightEditText_TextChanged;
            WarmupSetFinishedButton.Click += WarmupSetFinishedButton_Click;

            // set index and display the stuff
            warmup_set_index = 0;
        }

        protected override void OnPause()
        {
            base.OnPause();

            error_dialog?.Dismiss();
        }

        private void WarmupSetFinishedButton_Click(object sender, EventArgs e)
        {
            warmup_set_index++;

            if (Finished)
            {
                SetResult(Result.Ok);
                Finish();
                return;
            }

            StaticTimer.StartTimer(80, next_warmup_set.GetRestPeriod(first_exercise),
                Timer_Ticked, Timer_Elapsed);
        }

        private void WorkingSetWeightEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                working_weight = Int32.Parse(WorkingSetWeightEditText.Text);
            }
            catch (FormatException)
            {

            }
        }

        void SetCountDownText(int seconds_left)
        {
            TimeLeftTextView.Text = "Resting for another " +
                    seconds_left.ToString() + " seconds";
        }

        private void Timer_Ticked(int ticks_until_elapsed)
        {
            //RestPeriodSecondsRemaining = ticks_until_elapsed;
            RunOnUiThread(delegate
            {
                SetCountDownText(ticks_until_elapsed);
            });
        }

        private void Timer_Elapsed()
        {
            RunOnUiThread(delegate
            {
                TimeLeftTextView.Text = "TIME IS UP!!! *vibrate*" +
                    System.Environment.NewLine +
                    "Start your next set whenever you're ready";
            });
        }
    }
}