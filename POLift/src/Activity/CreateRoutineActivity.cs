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
    [Activity(Label = "Create Routine")]
    class CreateRoutineActivity : Activity
    {
        EditText RoutineTitleText;
        ListView ExercisesListView;
        Button AddExerciseButton;
        Button CreateRoutineButton;
       // ArrayAdapter<string> array_adapter;
        ExerciseAdapter exercise_adapter;
        
        List<Exercise> routine_exercises;

        static int SelectExerciseRequestCode = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreateRoutine);

            RoutineTitleText = FindViewById<EditText>(Resource.Id.RoutineTitleText);
            ExercisesListView = FindViewById<ListView>(Resource.Id.ExercisesListView);
            AddExerciseButton = FindViewById<Button>(Resource.Id.AddExerciseButton);
            CreateRoutineButton = FindViewById<Button>(Resource.Id.CreateRoutineButton);

            AddExerciseButton.Click += AddExerciseButton_Click;

            routine_exercises = new List<Exercise>();
            exercise_adapter = new ExerciseAdapter(this, routine_exercises);
            // default_items

            //array_adapter = new ArrayAdapter<string>(this,
            //    Android.Resource.Layout.SimpleListItem1, routine_exercises);
            ExercisesListView.Adapter = exercise_adapter; //array_adapter;

            CreateRoutineButton.Click += CreateRoutineButton_Click;
        }

        private void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            Intent result_intent = new Intent();
            Routine routine = new Routine(RoutineTitleText.Text, routine_exercises);

            result_intent.PutExtra("routine", routine.ToXml());

            SetResult(Result.Ok, result_intent);

            Finish();
        }

        protected void AddExerciseButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(ExerciseSelectActivity));
            StartActivityForResult(intent, SelectExerciseRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                Exercise selected_exercise = Exercise.FromXml(data.GetStringExtra("exercise"));
                // string 
                //array_adapter.Add();
                exercise_adapter.Add(selected_exercise);
                routine_exercises.Add(selected_exercise);
            }
        }
    }
}