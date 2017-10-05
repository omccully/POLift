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
    using Service;
    using Model;

    //[Activity(Label = "Perform routine")]
    public abstract class PerformRoutineBaseActivity : Activity
    {
        const string RestPeriodSecondsRemainingKey = "rest_period_seconds_remaining";

        protected TextView RoutineDetails;
        protected TextView NextExerciseView;
        protected Button ReportResultButton;

        protected TextView WeightLabel;
        protected EditText WeightEditText;

        protected TextView RepResultLabel;
        protected EditText RepResultEditText;
        

        protected TextView CountDownTextView;
        

        protected TextView PlateMathTextView;

        protected Button Sub30SecButton;
        protected Button Add30SecButton;
        protected Button SkipTimerButton;

        protected Exercise CurrentExercise;

        protected PlateMath CurrentPlateMath
        {
            get
            {
                if (CurrentExercise == null) return null;
                return PlateMath.PlateMathTypes[CurrentExercise.PlateMathID];
            }
        }

        protected int WeightInput
        {
            get
            {
                return Int32.Parse(WeightEditText.Text);
            }
            set
            {
                WeightEditText.Text = value.ToString();
            }
        }

        bool _TimerRunning = false;
        protected bool TimerRunning
        {
            get
            {
                return _TimerRunning;
            }
            set
            {
                _TimerRunning = value;

                if (value)
                {
                    Sub30SecButton.Enabled = true;
                    Add30SecButton.Enabled = true;
                    SkipTimerButton.Enabled = true;

                    ReportResultButton.Enabled = false;
                    RepResultEditText.Enabled = false;
                }
                else
                {
                    Sub30SecButton.Enabled = false;
                    Add30SecButton.Enabled = false;
                    SkipTimerButton.Enabled = false;

                    ReportResultButton.Enabled = true;
                    RepResultEditText.Enabled = true;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.PerformRoutine);

            RoutineDetails = FindViewById<TextView>(Resource.Id.RoutineDetails);
            NextExerciseView = FindViewById<TextView>(Resource.Id.NextExerciseView);
            ReportResultButton = FindViewById<Button>(Resource.Id.ReportResultButton);
            RepResultEditText = FindViewById<EditText>(Resource.Id.RepResultEditText);
            CountDownTextView = FindViewById<TextView>(Resource.Id.CountDownTextView);
            WeightEditText = FindViewById<EditText>(Resource.Id.WeightEditText);
            Sub30SecButton = FindViewById<Button>(Resource.Id.Sub30SecButton);
            Add30SecButton = FindViewById<Button>(Resource.Id.Add30SecButton);
            SkipTimerButton = FindViewById<Button>(Resource.Id.SkipTimerButton);
            PlateMathTextView = FindViewById<TextView>(Resource.Id.PlateMathTextView);
            RepResultLabel = FindViewById<TextView>(Resource.Id.RepResultLabel);
            WeightLabel = FindViewById<TextView>(Resource.Id.WeightLabel);

            RestoreTimerState(savedInstanceState);

            WeightEditText.TextChanged += WeightEditText_TextChanged;

            ReportResultButton.Click += ReportResultButton_Click;

            Sub30SecButton.Click += Sub30SecButton_Click;
            Add30SecButton.Click += Add30SecButton_Click;
            SkipTimerButton.Click += SkipTimerButton_Click;
        }

        protected void StartTimer(int seconds_left)
        {
            TimerRunning = true;
            StaticTimer.StartTimer(1000, seconds_left, Timer_Ticked, Timer_Elapsed);
            SetCountDownText(seconds_left);

        }

        protected abstract void ReportResultButton_Click(object sender, EventArgs e);

        protected virtual void WeightEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (CurrentPlateMath == null)
                {
                    PlateMathTextView.Text = "";
                }
                else
                {
                    string plate_counts_str = CurrentPlateMath.PlateCountsToString(WeightInput);
                    PlateMathTextView.Text = $" ({plate_counts_str})";
                }
            }
            catch (FormatException)
            {

            }
        }

        volatile int RestPeriodSecondsRemaining = 0;

        protected virtual void SaveTimerState(Bundle outState)
        {
            outState.PutInt(RestPeriodSecondsRemainingKey, RestPeriodSecondsRemaining);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            SaveTimerState(outState);
            base.OnSaveInstanceState(outState);
        }

        protected virtual void RestoreTimerState(Bundle savedInstanceState)
        {
            TimerRunning = StaticTimer.IsRunning;

            if (!TimerRunning) return;

            StaticTimer.TickedCallback += Timer_Ticked;
            StaticTimer.ElapsedCallback = Timer_Elapsed;

            if (savedInstanceState != null)
            {
                // screen was rotated... restore the timer textview
                RestPeriodSecondsRemaining = savedInstanceState.GetInt(
                    RestPeriodSecondsRemainingKey, -1);

                if (RestPeriodSecondsRemaining > 0)
                {
                    // timer was running last time

                    SetCountDownText(RestPeriodSecondsRemaining);
                }
            }

            // if no RestPeriodSecondsRemaining for whatever reason, 
            // we just wait for the next TimerTicked callback
        }

        protected virtual void SkipTimerButton_Click(object sender, EventArgs e)
        {
            StaticTimer.StopTimer();
            Timer_Elapsed();
        }

        protected virtual void Add30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.AddTicks(30);
        }

        protected virtual void Sub30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.SubtractTicks(30);
        }

        protected virtual void Timer_Ticked(int ticks_until_elapsed)
        {
            RestPeriodSecondsRemaining = ticks_until_elapsed;
            RunOnUiThread(delegate
            {
                SetCountDownText(ticks_until_elapsed);
            });
        }

        protected virtual void Timer_Elapsed()
        {
            RestPeriodSecondsRemaining = 0;
            RunOnUiThread(delegate
            {
                CountDownTextView.Text = "TIME IS UP!!! *vibrate*" +
                    System.Environment.NewLine +
                    "Start your next set whenever you're ready";

                TimerRunning = false;
            });

            Vibrator vibrator = (Vibrator)GetSystemService(Context.VibratorService);
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);

            // TODO: make notification here
        }

        protected virtual void SetCountDownText(int seconds_left)
        {
            CountDownTextView.Text = "Resting for another " +
                    seconds_left.ToString() + " seconds";
        }
    }
}