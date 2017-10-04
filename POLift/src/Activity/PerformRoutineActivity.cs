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
    public class PerformRoutineActivity : Activity
    {
        const string RestPeriodSecondsRemainingKey = "rest_period_seconds_remaining";

        TextView RoutineDetails;
        TextView NextExerciseView;
        Button ReportResultButton;
        EditText RepResultEditText;
        TextView CountDownTextView;
        EditText WeightEditText;

        TextView PlateMathTextView;

        Button Sub30SecButton;
        Button Add30SecButton;
        Button SkipTimerButton;

        Routine routine;

        AlertDialog dialog;

        bool AskForWarmupRoutine = true;

        RoutineResult _routine_result;
        RoutineResult routine_result
        {
            get
            {
                return _routine_result;
            }
            set
            {
                _routine_result = value;
                GetNextExerciseAndWeight();

                ExerciseResult last_ex_result =_routine_result.ExerciseResults.LastOrDefault();
                if(last_ex_result != null)
                {
                    int rest_period_seconds = last_ex_result.Exercise.RestPeriodSeconds;
                    TimeSpan rest_period_span = TimeSpan.FromSeconds(rest_period_seconds);
                    TimeSpan time_since_last_exercise = (DateTime.Now - last_ex_result.Time);
                    if (time_since_last_exercise < rest_period_span)
                    {
                        int remaining_timer = (int)(rest_period_seconds - time_since_last_exercise.TotalSeconds);

                        StaticTimer.StartTimer(1000, remaining_timer, Timer_Ticked, Timer_Elapsed);
                    }
                }

                if(AskForWarmupRoutine)
                {
                    dialog = Helpers.DisplayConfirmation(this, "Would you like to do a warmup routine?",
                    delegate
                    {
                        var intent = new Intent(this, typeof(WarmupRoutineActivity));
                        intent.PutExtra("exercise_id", exercise.ID);
                        intent.PutExtra("working_set_weight", Weight);
                        StartActivityForResult(intent, WARMUP_ROUTINE_REQUEST_CODE);
                    });
                }
                
            }
        }

        Exercise _exercise;
        Exercise exercise
        {
            get
            {
                return _exercise;
            }
            set
            {
                _exercise = value;
                if (value == null)
                {
                    NextExerciseView.Text = "Routine completed";
                }
                else
                {
                    NextExerciseView.Text = $"Exercise {routine_result.ResultCount}/"
                        + $"{ routine_result.ExerciseCount}: "
                        + value.ToString();
                    //throw new Exception();
                }

            }
        }

        int Weight
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
        bool TimerRunning
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

            WeightEditText.TextChanged += WeightEditText_TextChanged;

            if (savedInstanceState != null)
            {
                // screen was rotated...
                RestPeriodSecondsRemaining = savedInstanceState.GetInt(RestPeriodSecondsRemainingKey, -1);

                if (RestPeriodSecondsRemaining > 0)
                {
                    // timer was running last time

                    SetCountDownText(RestPeriodSecondsRemaining);

                    StaticTimer.TickedCallback += Timer_Ticked;
                    StaticTimer.ElapsedCallback = Timer_Elapsed;

                    TimerRunning = true;
                }

            }
                
            int routine_id = Intent.GetIntExtra("routine_id", -1);

            routine = POLDatabase.ReadByID<Routine>(routine_id);
            RoutineResult recent_uncompleted = RoutineResult.MostRecentUncompleted(routine);
            if (recent_uncompleted == null || 
                (DateTime.Now - recent_uncompleted.EndTime) > TimeSpan.FromDays(1))
            {
                // if there is no recent uncompleted routine result for this routine 
                // OR
                // if the most recent uncompleted routine result is more than a day ago, 
                // just start a new one

                routine_result = new RoutineResult(routine);
            }
            else
            {
                int resume_routine_result_id = (savedInstanceState == null ? -2 :
                    savedInstanceState.GetInt("resume_routine_result_id", -1));

                if(resume_routine_result_id == 0)
                {
                    // a routine result was started but has no contents
                    AskForWarmupRoutine = false;
                    routine_result = new RoutineResult(routine);
                }
                else if (recent_uncompleted.ID == resume_routine_result_id)
                {
                    // restore saved state
                    routine_result = recent_uncompleted;
                }
                else
                {
                    Helpers.DisplayConfirmation(this, "You did not fiish this routine on " +
                        recent_uncompleted.EndTime.ToString() + ". Would you like to resume it?",
                        delegate
                        {
                            // yes
                            AskForWarmupRoutine = false;
                            routine_result = recent_uncompleted;
                        },
                        delegate
                        {
                            // no
                            routine_result = new RoutineResult(routine);
                        }
                    );
                }
            }

            RoutineDetails.Text = ExtendedRoutineDetails(routine);

            ReportResultButton.Click += ReportResultButton_Click;

            Sub30SecButton.Click += Sub30SecButton_Click;
            Add30SecButton.Click += Add30SecButton_Click;
            SkipTimerButton.Click += SkipTimerButton_Click;

            if(!TimerRunning) 
                TimerRunning = false; // set textboxes properly;

            /*if(routine_result != null && savedInstanceState == null)
            {
                // not waiting for user to choose yes/no for resuming routine
                // and screen didn't rotate

                var intent = new Intent(this, typeof(WarmupRoutineActivity));


            }*/
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }

        private void WeightEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                int weight = Weight;

                if (exercise == null) return;
                PlateMath plate_math = PlateMath.PlateMathTypes[exercise.PlateMathID];

                PlateMathTextView.Text = " (" + plate_math.PlateCountsToString(weight) + ")";
            }
            catch(FormatException)
            {

            }
        }

        const int WARMUP_ROUTINE_REQUEST_CODE = 100;

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == WARMUP_ROUTINE_REQUEST_CODE)
            {
                if(exercise != null)
                {
                    // start timer once warmup routine is finished.
                    StartRestPeriod();
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();


        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            if(routine_result != null)
            {
                System.Diagnostics.Debug.WriteLine("set resume_routine_result_id = " + routine_result.ID);
                outState.PutInt("resume_routine_result_id", routine_result.ID);
            }

            outState.PutInt(RestPeriodSecondsRemainingKey, RestPeriodSecondsRemaining);
            
            base.OnSaveInstanceState(outState);
        }

        private void SkipTimerButton_Click(object sender, EventArgs e)
        {
            StaticTimer.StopTimer();
            Timer_Elapsed();
        }

        private void Add30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.AddTicks(30);
        }

        private void Sub30SecButton_Click(object sender, EventArgs e)
        {
            StaticTimer.SubtractTicks(30);
        }

        private void ReportResultButton_Click(object sender, EventArgs e)
        {
            // user submitted a result for this exercise

            // get number of reps and clear the text box
            int reps = 0;
            int weight = Weight;
            try
            {
                weight = Weight;
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
                new ExerciseResult(exercise, weight, reps);
            POLDatabase.Insert(ex_result);
            routine_result.ReportExerciseResult(ex_result);

            // insert or update the routine result after EVERY new result
            // just in case the app crashes or something
            POLDatabase.InsertOrUpdateByID(routine_result);

            if (routine_result.Completed)
            {
                // no more exercises
                ReturnRoutineResult(routine_result);
                return;
            }

            // update Weight and exercise
            GetNextExerciseAndWeight();
            // rest period is based on the NEXT exercise's rest period

            StartRestPeriod();
        }

        void StartRestPeriod()
        {
            // execute rest period, disable button and text box
            TimerRunning = true;

            StaticTimer.StartTimer(1000, exercise.RestPeriodSeconds, Timer_Ticked, Timer_Elapsed);
        }

        void SetCountDownText(int seconds_left)
        {
            CountDownTextView.Text = "Resting for another " +
                    seconds_left.ToString() + " seconds";
        }

        volatile int RestPeriodSecondsRemaining = 0;
        private void Timer_Ticked(int ticks_until_elapsed)
        {
            RestPeriodSecondsRemaining = ticks_until_elapsed;
            RunOnUiThread(delegate
            {
                SetCountDownText(ticks_until_elapsed);
            });
        }

        private void Timer_Elapsed()
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

        string ExtendedRoutineDetails(Routine routine)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(routine.Name);
            sb.Append(":");
            sb.Append(System.Environment.NewLine);

            return sb.ToString();
        }

        void GetNextExerciseAndWeight()
        {
            // get next exercise
            exercise = routine_result.NextExercise;

            if (exercise != null)
            {
                Weight = exercise.NextWeight;
            }
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