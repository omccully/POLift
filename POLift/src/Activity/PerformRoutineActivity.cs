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
        TextView RoutineDetails;
        TextView NextExerciseView;
        Button ReportResultButton;
        EditText RepResultEditText;
        TextView CountDownTextView;
        EditText WeightEditText;

        Routine routine;
        
        RoutineResult routine_result;

        Timer timer;

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
                if(value == null)
                {
                    NextExerciseView.Text = "Routine completed";
                }
                else
                {
                    NextExerciseView.Text = "Exercise: " + value.ToString();
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

            int id = Intent.GetIntExtra("routine_id", -1);

            routine = POLDatabase.ReadByID<Routine>(id);
            routine_result = new RoutineResult(routine);

            RoutineDetails.Text = ExtendedRoutineDetails(routine);

            ReportResultButton.Click += ReportResultButton_Click;

            // update Weight and exercise
            GetNextExerciseAndWeight();

            timer = new Timer(/*1000.0*/ 50);
            timer.Elapsed += Timer_Elapsed;
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
            catch(FormatException)
            {
                Helpers.DisplayError(this, 
                    "You must fill out the weight and rep count with integers");
                return;
            }

            RepResultEditText.Text = "";

            // execute rest period, disable button and text box
            ReportResultButton.Enabled = false;
            RepResultEditText.Enabled = false;

            // report the exercise result
            ExerciseResult ex_result =
                new ExerciseResult(exercise, weight, reps);
            POLDatabase.Insert(ex_result);
            routine_result.ReportExerciseResult(ex_result);

            // update Weight and exercise
            GetNextExerciseAndWeight();

            seconds_left = exercise.RestPeriodSeconds;
            timer.Start();
        }

        int seconds_left = 0;

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(seconds_left > 0)
            {
                // the timer has not finished yet
                seconds_left--;

                RunOnUiThread(delegate
                {
                    CountDownTextView.Text = "Resting for another " +
                        seconds_left.ToString() + " seconds";
                });
            }
            else
            {
                // time is up

                RunOnUiThread(delegate
                {
                    CountDownTextView.Text = "TIME IS UP!!! *vibrate*" + 
                        System.Environment.NewLine + 
                        "Start your next set whenever you're ready";

                    ReportResultButton.Enabled = true;
                    RepResultEditText.Enabled = true;
                });
            }
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

            if(exercise == null)
            {
                // no more exercises!
                // this is where the activity comes to an end
                POLDatabase.Insert(routine_result);
                ReturnRoutineResult(routine_result);
                // TODO: insert this routine result as soon as first result is recorded
                // and just update it as time goes on
            }

            // should get the correct new weight from the above
            // inserted ExerciseResult if we've progressed
            Weight = exercise.NextWeight;
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