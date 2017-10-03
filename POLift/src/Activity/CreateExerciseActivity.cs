using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;

namespace POLift
{
    using Android.Preferences;
    using Android.Views.InputMethods;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.CreateExercise);
            
            ExerciseNameText = FindViewById<EditText>(Resource.Id.ExerciseNameText);
            RepRangeMaxText = FindViewById<EditText>(Resource.Id.RepRangeMaxText);
            CreateExerciseButton = FindViewById<Button>(Resource.Id.CreateExerciseButton);
            RestPeriodSecondsText = FindViewById<EditText>(Resource.Id.RestPeriodSecondsTextBox);
            WeightIncrementText = FindViewById<EditText>(Resource.Id.WeightIncrementTextBox);
            SelectMathTypeSpinner = FindViewById<Spinner>(Resource.Id.SelectMathTypeSpinner);

            SelectMathTypeSpinner.Adapter = new PlateMathTypeAdapter(this, PlateMath.PlateMathTypes);

            int edit_exercise_id = Intent.GetIntExtra("edit_exercise_id", -1);
            if(edit_exercise_id == -1)
            {
                LoadPreferences();
            }
            else
            {
                Exercise exercise = POLDatabase.ReadByID<Exercise>(edit_exercise_id);

                ExerciseNameText.Text = exercise.Name;
                RepRangeMaxText.Text = exercise.MaxRepCount.ToString();
                RestPeriodSecondsText.Text = exercise.RestPeriodSeconds.ToString();
                WeightIncrementText.Text = exercise.WeightIncrement.ToString();

                SelectMathTypeSpinner.SetSelection(exercise.PlateMathID);
            }

            // TODO: fix this to make name text box focused when activity starts
            ExerciseNameText.RequestFocus();
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(ExerciseNameText, ShowFlags.Implicit);

            CreateExerciseButton.Click += CreateExerciseButton_Click;
        }

        protected void CreateExerciseButton_Click(object sender, EventArgs e)
        {
            try
            {
                string name = ExerciseNameText.Text;
                int max_reps = Int32.Parse(RepRangeMaxText.Text);
                int weight_increment = Int32.Parse(WeightIncrementText.Text);
                int rest_period_s = Int32.Parse(RestPeriodSecondsText.Text);
                PlateMath plate_math = null;

                int pos = SelectMathTypeSpinner.SelectedItemPosition;
                plate_math = PlateMath.PlateMathTypes[pos];

                Exercise ex = new Exercise(name, max_reps, weight_increment, 
                    rest_period_s, plate_math);
                
                // sets ex.ID
                POLDatabase.InsertOrUndeleteAndUpdate(ex);

                SavePreferences();
                ReturnExercise(ex);
            }
            catch (ArgumentException ae)
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(this);
                dialog.SetMessage(ae.Message);
                dialog.SetNegativeButton("Ok", delegate { });
                dialog.Show();
            }
        }

        void ReturnExercise(Exercise exercise)
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
            editor.Apply();
        }

        void LoadPreferences()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            RepRangeMaxText.Text = prefs.GetString("create_exercise_max_reps", "");
            WeightIncrementText.Text = prefs.GetString("create_exercise_weight_increment", "5");
            RestPeriodSecondsText.Text = prefs.GetString("create_exercise_rest_period_seconds", "120");
        }
    }
}

