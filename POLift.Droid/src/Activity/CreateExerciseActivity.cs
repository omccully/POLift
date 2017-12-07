﻿using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Microsoft.Practices.Unity;
using Android.Preferences;
using Android.Views.InputMethods;

namespace POLift.Droid
{
   
    using Core.Model;
    using Core.Service;

    [Activity(Label = "Create Exercise")]
    public class CreateExerciseActivity : Activity
    {
        EditText ExerciseNameText;
        EditText RepRangeMaxText;
        Button CreateExerciseButton;
        EditText RestPeriodSecondsText;
        EditText WeightIncrementText;
        Spinner SelectMathTypeSpinner;
        TextView ExerciseDetailsTextView;
        EditText ConsecutiveSetsForWeightIncrease;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.CreateExercise);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            ExerciseNameText = FindViewById<EditText>(Resource.Id.ExerciseNameText);
            RepRangeMaxText = FindViewById<EditText>(Resource.Id.RepRangeMaxText);
            CreateExerciseButton = FindViewById<Button>(Resource.Id.CreateExerciseButton);
            RestPeriodSecondsText = FindViewById<EditText>(Resource.Id.RestPeriodSecondsTextBox);
            WeightIncrementText = FindViewById<EditText>(Resource.Id.WeightIncrementTextBox);
            SelectMathTypeSpinner = FindViewById<Spinner>(Resource.Id.SelectMathTypeSpinner);
            ExerciseDetailsTextView = FindViewById<TextView>(Resource.Id.ExerciseDetailsTextView);
            ConsecutiveSetsForWeightIncrease = FindViewById<EditText>(Resource.Id.ConsecutiveSetsForWeightIncrease);

            SelectMathTypeSpinner.Adapter = new PlateMathTypeAdapter(this, PlateMath.PlateMathTypes);

            ExerciseNameText.TextChanged += ExerciseParameter_TextChanged;
            RepRangeMaxText.TextChanged += ExerciseParameter_TextChanged;
            RestPeriodSecondsText.TextChanged += ExerciseParameter_TextChanged;
            WeightIncrementText.TextChanged += ExerciseParameter_TextChanged;
            ConsecutiveSetsForWeightIncrease.TextChanged += ExerciseParameter_TextChanged;

            int edit_exercise_id = Intent.GetIntExtra("edit_exercise_id", -1);
            if(edit_exercise_id == -1)
            {
                LoadPreferences();
            }
            else
            {
                IExercise exercise = Database.ReadByID<Exercise>(edit_exercise_id);

                ExerciseNameText.Text = exercise.Name;
                RepRangeMaxText.Text = exercise.MaxRepCount.ToString();
                RestPeriodSecondsText.Text = exercise.RestPeriodSeconds.ToString();
                WeightIncrementText.Text = exercise.WeightIncrement.ToString();
                ConsecutiveSetsForWeightIncrease.Text = 
                    exercise.ConsecutiveSetsForWeightIncrease.ToString();

                SelectMathTypeSpinner.SetSelection(exercise.PlateMathID);
            }

            UpdateExerciseDetails();

            // TODO: fix this to make name text box focused when activity starts
            ExerciseNameText.RequestFocus();
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(ExerciseNameText, ShowFlags.Implicit);

            CreateExerciseButton.Click += CreateExerciseButton_Click;
        }

        private void ExerciseParameter_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            UpdateExerciseDetails();
        }

        void UpdateExerciseDetails()
        {
            // You will as many reps of <exercise name>
            // as you can for each set. If you get <max reps> reps,
            // you will increase the weight by <weight increment>

            int cs = -1;
            bool cs_success = Int32.TryParse(ConsecutiveSetsForWeightIncrease.Text, out cs);

            StringBuilder builder = new StringBuilder();

            builder.Append("Explanation of your input: ");

            if (!cs_success || cs >= 1)
            {
                builder.Append("You will try to get as many reps of ")
                .Append(EnsureString(ExerciseNameText.Text, "<exercise name>"))
                .Append(" as you can for each set. If you get ")
                .Append(EnsureInt(RepRangeMaxText.Text, "<reps>"))
                .Append(" reps");

                if(cs > 1)
                {
                    builder.Append(" for ")
                        .Append(EnsureInt(ConsecutiveSetsForWeightIncrease.Text, "<consecutive sets>"))
                        .Append(" sets in a row");
                }

                builder.Append(", you will increase the weight by ")
                   .Append(EnsureFloat(WeightIncrementText.Text, "<weight increment>"))
                   .Append(" for your next set. ");
            }
            else if(cs == 0)
            {
                builder.Append("You will try to get ")
                    .Append(EnsureInt(RepRangeMaxText.Text, "<reps>"))
                    .Append(" reps for each set. You will increase the weight manually ")
                    .Append("(it's normally recommended to let the app increase weight automatically")
                    .Append(" using setting \"consecutive sets\" to a number greater than 0). ");
            }


           builder.Append("You will rest for ")
                .Append(EnsureInt(RestPeriodSecondsText.Text, "<rest period seconds>"))
                .Append(" seconds in between sets.");

            ExerciseDetailsTextView.Text = builder.ToString();
        }

        string EnsureString(string input_text, string fail_text = "?")
        {
            return String.IsNullOrWhiteSpace(input_text) ? fail_text : input_text;
        }

        string EnsureInt(string input_text, string fail_text = "?")
        {
            try
            {
                return Int32.Parse(input_text).ToString();
            }
            catch
            {
                return fail_text;
            }
        }

        string EnsureFloat(string input_text, string fail_text = "?")
        {
            try
            {
                return Single.Parse(input_text).ToString();
            }
            catch
            {
                return fail_text;
            }
        }

        protected void CreateExerciseButton_Click(object sender, EventArgs e)
        {
            try
            {
                string name = ExerciseNameText.Text;
                int max_reps = Int32.Parse(RepRangeMaxText.Text);
                float weight_increment = Single.Parse(WeightIncrementText.Text);
                int rest_period_s = Int32.Parse(RestPeriodSecondsText.Text);
                int consecutive_sets = Int32.Parse(ConsecutiveSetsForWeightIncrease.Text);

                IPlateMath plate_math = null;

                int pos = SelectMathTypeSpinner.SelectedItemPosition;
                plate_math = PlateMath.PlateMathTypes[pos];

                Exercise ex = new Exercise(name, max_reps, weight_increment, 
                    rest_period_s, plate_math);
                ex.Database = Database;
                ex.ConsecutiveSetsForWeightIncrease = consecutive_sets;

                // sets ex.ID
                Database.InsertOrUndeleteAndUpdate(ex);

                SavePreferences();
                ReturnExercise(ex);
            }
            catch(FormatException)
            {
                // FormatException for int parsing
                Toast.MakeText(this, "Numerical fields must be integers", 
                    ToastLength.Long).Show();
            }
            catch (ArgumentException ae)
            {
                // ArgumentException for Exercise constructor
                Toast.MakeText(this, ae.Message, ToastLength.Long).Show();
            }
        }

        void ReturnExercise(IExercise exercise)
        {
            ReturnExercise(exercise.ID);
        }

        void ReturnExercise(int ID)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("exercise_id", ID);
            SetResult(Result.Ok, result_intent);
            Finish();
        }

        void SavePreferences()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutString("create_exercise_max_reps", RepRangeMaxText.Text);
            editor.PutString("create_exercise_weight_increment", WeightIncrementText.Text);
            editor.PutString("create_exercise_rest_period_seconds", RestPeriodSecondsText.Text);
            editor.PutString("consecutive_sets_for_weight_increase", ConsecutiveSetsForWeightIncrease.Text);
            editor.PutBoolean("exercise_created_since_last_difficulty_regeneration", true);
            editor.Apply();
        }

        void LoadPreferences()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            RepRangeMaxText.Text = prefs.GetString("create_exercise_max_reps", "8");
            WeightIncrementText.Text = prefs.GetString("create_exercise_weight_increment", "5");
            RestPeriodSecondsText.Text = prefs.GetString("create_exercise_rest_period_seconds", "120");
            ConsecutiveSetsForWeightIncrease.Text = prefs.GetString("consecutive_sets_for_weight_increase", "1");
        }
    }
}

