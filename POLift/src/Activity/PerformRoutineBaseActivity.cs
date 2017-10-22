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
using Android.Support.Compat;

namespace POLift
{
    using Service;
    using Model;
    using Android.Support.V4.App;

    //[Activity(Label = "Perform routine")]
    public abstract class PerformRoutineBaseActivity : Activity
    {
        const int TimerNotificationID = 500;
        const string RestPeriodSecondsRemainingKey = "rest_period_seconds_remaining";

        protected TextView RoutineDetails;
        protected TextView NextExerciseView;

        protected TextView WeightLabel;
        protected EditText WeightEditText;
        protected TextView PlateMathTextView;

        protected TextView RepResultLabel;
        protected EditText RepResultEditText;

        protected TextView NextWarmupView;

        protected Button ReportResultButton;

        protected TextView CountDownTextView;
        protected Button Sub30SecButton;
        protected Button Add30SecButton;
        protected Button SkipTimerButton;
        
        protected Button ModifyRestOfRoutineButton;

        protected Button IMadeAMistakeButton;

        protected IExercise CurrentExercise;

        protected IPlateMath CurrentPlateMath
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

        /*bool _TimerRunning = false;
        protected bool TimerRunning
        {
            get
            {
                return _TimerRunning;
            }
            set
            {
                _TimerRunning = value;

                UpdateGUIByTimerState();
            }
        }*/

        void UpdateGUIByTimerState()
        {
            if(StaticTimer.IsRunningPositive)
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
            ModifyRestOfRoutineButton = FindViewById<Button>(Resource.Id.ModifyRestOfRoutineButton);
            NextWarmupView = FindViewById<TextView>(Resource.Id.NextWarmupView);
            IMadeAMistakeButton = FindViewById<Button>(Resource.Id.IMadeAMistakeButton);

            RestoreTimerState(savedInstanceState);

            WeightEditText.TextChanged += WeightEditText_TextChanged;

            ReportResultButton.Click += ReportResultButton_Click;

            Sub30SecButton.Click += Sub30SecButton_Click;
            Add30SecButton.Click += Add30SecButton_Click;
            SkipTimerButton.Click += SkipTimerButton_Click;
        }

        protected void StartTimer(int seconds_left)
        {
            //TimerRunning = true;
            StaticTimer.StartTimer(1000, seconds_left, Timer_Ticked, Timer_Elapsed);
            SetCountDownText(seconds_left);

            UpdateGUIByTimerState();

            NotificationManager mNotificationManager =
               (NotificationManager)GetSystemService(Context.NotificationService);

            mNotificationManager.Cancel(TimerNotificationID);

            AddSecCount = 0;
            SubSecCount = 0;
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


        Bundle GetTimerState()
        {
            Bundle bundle = new Bundle();

            bundle.PutInt(RestPeriodSecondsRemainingKey, RestPeriodSecondsRemaining);
            return bundle;
        }

        protected virtual void SaveTimerState(Bundle outState)
        {
           // outState.PutInt(RestPeriodSecondsRemainingKey, RestPeriodSecondsRemaining);

            outState.PutAll(GetTimerState());
            int test = outState.GetInt(RestPeriodSecondsRemainingKey, -1);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            SaveTimerState(outState);
            base.OnSaveInstanceState(outState);
        }

        protected virtual void RestoreTimerState(Bundle savedInstanceState)
        {
            //TimerRunning = StaticTimer.IsRunning;
            //TimerRunning = StaticTimer.IsRunningPositive;

            if (!StaticTimer.IsRunning) return;

            StaticTimer.TickedCallback += Timer_Ticked;
            StaticTimer.ElapsedCallback = Timer_Elapsed;

            int intent_rpsr = Intent.GetIntExtra(RestPeriodSecondsRemainingKey, -1);
            RestPeriodSecondsRemaining = intent_rpsr;

            if (savedInstanceState != null)
            {
                // screen was rotated... restore the timer textview
                RestPeriodSecondsRemaining = savedInstanceState.GetInt(
                    RestPeriodSecondsRemainingKey, RestPeriodSecondsRemaining);
            }

            if (RestPeriodSecondsRemaining > 0)
            {
                // timer was running last time

                SetCountDownText(RestPeriodSecondsRemaining);
            }

            // if no RestPeriodSecondsRemaining for whatever reason, 
            // we just wait for the next TimerTicked callback
        }

        protected virtual void SkipTimerButton_Click(object sender, EventArgs e)
        {
            RestPeriodSecondsRemaining = 0;

            StaticTimer.StopTimer();

            CountDownTextView.Text = "Timer skipped. " + System.Environment.NewLine +
                 "Start your next set whenever you're ready";
            CountDownTextView.SetTextColor(Android.Graphics.Color.White);

            UpdateGUIByTimerState();
            Vibrate();
            
            //Timer_Elapsed();
        }

        void CancelTheTimerControlCounts()
        {
            int min = Math.Min(AddSecCount, SubSecCount);
            if(min > 0)
            {
                AddSecCount -= min;
                SubSecCount -= min;
            }
        }

        int _add_sec_count = 0;
        int AddSecCount
        {
            get
            {
                return _add_sec_count;
            }
            set
            {
                _add_sec_count = value;

                // this may modify _add_sec_count
                CancelTheTimerControlCounts();

                if (_add_sec_count == 0)
                {
                    Add30SecButton.Text = "+30 sec";
                }
                else
                {
                    Add30SecButton.Text = $"+30 sec (x{_add_sec_count})";
                }
                 
            }
        }

        int _sub_sec_count = 0;
        int SubSecCount
        {
            get
            {
                return _sub_sec_count;
            }
            set
            {
                _sub_sec_count = value;

                // this may modify _add_sec_count
                CancelTheTimerControlCounts();

                if (_sub_sec_count == 0)
                {
                    Sub30SecButton.Text = "-30 sec";
                }
                else
                {
                    Sub30SecButton.Text = $"-30 sec (x{_sub_sec_count})";
                }
            }
        }

        protected virtual void Add30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.AddTicks(30);
            AddSecCount++;
        }

        protected virtual void Sub30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.SubtractTicks(30);
            SubSecCount++;
        }

        protected virtual void Timer_Ticked(int ticks_until_elapsed)
        {
            RestPeriodSecondsRemaining = ticks_until_elapsed;
            RunOnUiThread(delegate
            {
                SetCountDownText(ticks_until_elapsed);
            });
        }

        protected virtual void SaveStateToIntent(Intent intent)
        {
            intent.PutExtras(GetTimerState());
        }

        protected virtual void Timer_Elapsed()
        {
            //RestPeriodSecondsRemaining = 0;
            RunOnUiThread(delegate
            {
                /*CountDownTextView.Text = "TIME IS UP!!! *vibrate*" +
                    System.Environment.NewLine +
                    "Start your next set whenever you're ready";*/

                UpdateGUIByTimerState();
            });

            Vibrate();

            // TODO: fix notification here
            StartTimerNotification();
        }

        void StartTimerNotification()
        {
            Intent result_intent = new Intent(this, this.GetType());
            SaveStateToIntent(result_intent);

            PendingIntent resultPendingIntent = TaskStackBuilder.Create(this)
                //.AddParentStack(this)
                //.AddNextIntent(result_intent)
                .AddNextIntentWithParentStack(result_intent)
                .GetPendingIntent(500, (int)PendingIntentFlags.UpdateCurrent);

            NotificationCompat.Builder n_builder = new NotificationCompat.Builder(this)
               .SetSmallIcon(Resource.Drawable.timer_white)
               .SetContentTitle("Lifting rest period finished")
               .SetContentText("Start your next set whenever you are ready")
               .SetContentIntent(resultPendingIntent);

            NotificationManager mNotificationManager =
                (NotificationManager)GetSystemService(Context.NotificationService);

            mNotificationManager.Notify(TimerNotificationID, n_builder.Build());
        }

        void Vibrate()
        {
            Vibrator vibrator = (Vibrator)GetSystemService(Context.VibratorService);
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);
        }

        protected virtual void SetCountDownText(int seconds_left)
        {
            if(seconds_left > 0)
            {
                CountDownTextView.Text = "Resting for another " +
                   seconds_left + " seconds.";
                CountDownTextView.SetTextColor(Android.Graphics.Color.Orange);
            }
            else
            {

                CountDownTextView.Text = "The timer has been done for " + 
                    Math.Abs(seconds_left) + " seconds." + System.Environment.NewLine +
                    "Start your next set whenever you're ready";
                CountDownTextView.SetTextColor(Android.Graphics.Color.Green);
            }
            
        }

        protected virtual void BuildArtificialTaskStack(TaskStackBuilder stackBuilder)
        {
            Intent main_intent = new Intent(this, typeof(MainActivity));

        }
    }
}