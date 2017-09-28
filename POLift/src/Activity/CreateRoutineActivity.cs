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
    using Adapter;

    [Activity(Label = "Create Routine")]
    class CreateRoutineActivity : Activity
    {
        EditText RoutineTitleText;
        ListView ExercisesListView;
        Button AddExerciseButton;
        Button CreateRoutineButton;

        //ExerciseAdapter exercise_adapter;
        ExerciseSetsAdapter exercise_sets_adapter;
        
        //List<Exercise> routine_exercises;
        List<ExerciseSets> routine_exercise_sets;

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

            //routine_exercises = new List<Exercise>();
            // 
            routine_exercise_sets = new List<ExerciseSets>();

           // exercise_adapter = new ExerciseAdapter(this, routine_exercises);
            exercise_sets_adapter = new ExerciseSetsAdapter(this, routine_exercise_sets);


            // ExercisesListView.Adapter = exercise_adapter; 
            ExercisesListView.Adapter = exercise_sets_adapter;

            CreateRoutineButton.Click += CreateRoutineButton_Click;
        }

        private void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            try
            {
                Routine routine = new Routine(RoutineTitleText.Text, routine_exercise_sets);
                POLDatabase.Insert(routine);
                ReturnRoutine(routine);
            }
            catch(ArgumentException ae)
            {
                Helpers.DisplayError(this, ae.Message);
            }
        }

        void ReturnRoutine(Routine routine)
        {
            ReturnRoutine(routine.ID);
        }

        void ReturnRoutine(int routine_id)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("routine_id", routine_id);
            SetResult(Result.Ok, result_intent);
            Finish();
        }

        protected void AddExerciseButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(SelectExerciseActivity));
            StartActivityForResult(intent, SelectExerciseRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            const int DEFAULT_SET_COUNT = 3;

            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                //Exercise selected_exercise = Exercise.FromXml(data.GetStringExtra("exercise"));

                int id = data.GetIntExtra("exercise_id", -1);
                if (id == -1) return;
                Exercise selected_exercise = POLDatabase.ReadByID<Exercise>(id);

                //exercise_adapter.Add(selected_exercise);
                //routine_exercises.Add(selected_exercise);

                ExerciseSets es = new ExerciseSets(selected_exercise, DEFAULT_SET_COUNT);
                POLDatabase.Insert(es);
                routine_exercise_sets.Add(es);
                exercise_sets_adapter.Add(es);
            }
        }
    }
}