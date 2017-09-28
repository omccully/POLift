using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.CreateExercise);
            
            ExerciseNameText = FindViewById<EditText>(Resource.Id.ExerciseNameText);
            RepRangeMaxText = FindViewById<EditText>(Resource.Id.RepRangeMaxText);
            CreateExerciseButton = FindViewById<Button>(Resource.Id.CreateExerciseButton);

            CreateExerciseButton.Click += CreateExerciseButton_Click;
        }

        protected void CreateExerciseButton_Click(object sender, EventArgs e)
        {
            try
            {
                Exercise ex = new Exercise(ExerciseNameText.Text, Int32.Parse(RepRangeMaxText.Text));
                POLDatabase.Insert(ex); // sets ex.ID
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

    }
}

