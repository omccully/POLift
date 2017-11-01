using Android.App;
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

namespace POLift
{
   
    using Model;
    using Service;

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

            StringBuilder builder = new StringBuilder();
            builder.Append("Explanation of your input: You will try to get as many reps of ")
                .Append(EnsureString(ExerciseNameText.Text, "<exercise name>"))
                .Append(" as you can for each set. If you get ")
                .Append(EnsureInt(RepRangeMaxText.Text, "<max reps>"))
                .Append(" reps")

                // if in the future I want to hide this when CSFWI is 0 or 1
                .Append(" for ")
                .Append(EnsureInt(ConsecutiveSetsForWeightIncrease.Text, "<consecutive sets>"))
                .Append(" sets in a row")

                .Append(", you will increase the weight by ")
                .Append(EnsureInt(WeightIncrementText.Text, "<weight increment>"))
                .Append(" for your next set. You will rest for ")
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

        protected void CreateExerciseButton_Click(object sender, EventArgs e)
        {
            try
            {
                string name = ExerciseNameText.Text;
                int max_reps = Int32.Parse(RepRangeMaxText.Text);
                int weight_increment = Int32.Parse(WeightIncrementText.Text);
                int rest_period_s = Int32.Parse(RestPeriodSecondsText.Text);
                int consecutive_sets = Int32.Parse(ConsecutiveSetsForWeightIncrease.Text);

                IPlateMath plate_math = null;

                int pos = SelectMathTypeSpinner.SelectedItemPosition;
                plate_math = PlateMath.PlateMathTypes[pos];

                Exercise ex = new Exercise(name, max_reps, weight_increment, 
                    rest_period_s, plate_math);
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

