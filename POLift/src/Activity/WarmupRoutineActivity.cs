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

    [Activity(Label = "Warmup")]
    public class WarmupRoutineActivity : Activity
    {
        Button WarmupSetFinishedButton;
        EditText WorkingSetWeightEditText;
        TextView WarmupRoutineTextView;
        TextView TimeLeftTextView;

        Exercise first_exercise = null;

        int _working_weight = 0;
        int working_weight
        {
            get
            {
                return _working_weight;
            }
            set
            {
                _working_weight = value;
                RefreshWarmupInfo();
            }
        }


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
                RefreshWarmupInfo();
            }
        }

        void RefreshWarmupInfo()
        {
            if (_warmup_set_index >= warmup_sets.Length)
            {
                WarmupRoutineTextView.Text = "Finished";
                return;
            }

            WarmupSet ws = warmup_sets[_warmup_set_index];
            string txt = $"{ws.Reps} reps of {first_exercise.Name} at a weight of ";

            int weight = ws.GetWeight(first_exercise, working_weight);
            txt += weight.ToString();
            if (first_exercise.PlateMath != null)
            {
                txt += " (" + first_exercise.PlateMath.PlateCountsToString(weight) + ")";
            }

            if (!String.IsNullOrEmpty(ws.Notes))
            {
                txt += $" ({ws.Notes})";
            }

            WarmupRoutineTextView.Text = txt;
            next_warmup_set = ws;
        }

        bool Finished
        {
            get
            {
                return warmup_set_index >= warmup_sets.Length;
            }
        }

        Dialog error_dialog;
        Dialog back_button_dialog;

        public override void OnBackPressed()
        {
            back_button_dialog = Helpers.DisplayConfirmation(this, 
                "Are you sure you want to end this warmup session? " +
                " You will lose all of your progress in this warmup.",
                delegate
                {
                    base.OnBackPressed();
                });
        }

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
            error_dialog?.Dismiss();
            back_button_dialog?.Dismiss();

            base.OnPause();
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

            StaticTimer.StartTimer(1000, next_warmup_set.GetRestPeriod(first_exercise),
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