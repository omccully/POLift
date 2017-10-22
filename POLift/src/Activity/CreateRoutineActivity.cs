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

using Microsoft.Practices.Unity;

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

        ExerciseSetsAdapter exercise_sets_adapter;
        
        static int SelectExerciseRequestCode = 1;

        IRoutine RoutineToDeleteIfDifferent = null;

        int LockedSetCount = 0;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreateRoutine);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutineTitleText = FindViewById<EditText>(Resource.Id.RoutineTitleText);
            ExercisesListView = FindViewById<ListView>(Resource.Id.ExercisesListView);
            AddExerciseButton = FindViewById<Button>(Resource.Id.AddExerciseButton);
            CreateRoutineButton = FindViewById<Button>(Resource.Id.CreateRoutineButton);

            int edit_routine_id = Intent.GetIntExtra("edit_routine_id", -1);

            List<IExerciseSets> exercise_sets = new List<IExerciseSets>();
            if (savedInstanceState != null)
            {
                int[] ids = savedInstanceState.GetIntArray(EXERCISE_SETS_IDS_KEY);
                if(ids != null)
                {
                    exercise_sets = new List<IExerciseSets>(Database.ParseIDs<ExerciseSets>(ids));
                }
                LockedSetCount = savedInstanceState.GetInt("exercise_sets_locked", 0);
            }
            else if (edit_routine_id != -1)
            {
                Routine routine = Database.ReadByID<Routine>(edit_routine_id);

                RoutineTitleText.Text = routine.Name;

                // take the exercises list, split it at location exercises_locked
                // ExerciseSets.Group(Exercise[] exercises)
                int locked_count = Intent.GetIntExtra("exercises_locked", 0);

                IEnumerable<IExercise> all_exercises = routine.Exercises;

                IEnumerable<IExerciseSets> locked_sets = 
                    ExerciseSets.Group(all_exercises.Take(locked_count), Database);
                IEnumerable<IExerciseSets> unlocked_sets = 
                    ExerciseSets.Group(all_exercises.Skip(locked_count), Database);
                LockedSetCount = locked_sets.Count();
                exercise_sets.AddRange(locked_sets);
                exercise_sets.AddRange(unlocked_sets);

                // TODO: this may need to be persisted
                RoutineToDeleteIfDifferent = routine;
            }

            exercise_sets_adapter = new ExerciseSetsAdapter(this, 
                exercise_sets, LockedSetCount);

            AddExerciseButton.Click += AddExerciseButton_Click;

            ExercisesListView.Adapter = exercise_sets_adapter;

            CreateRoutineButton.Click += CreateRoutineButton_Click;

            ExercisesListView.ItemLongClick += ExercisesListView_ItemLongClick;
            ExercisesListView.Drag += ExercisesListView_Drag;
        }

        private void ExercisesListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("item long click");
            //ExercisesListView.StartDragAndDrop()
        }

        private void ExercisesListView_Drag(object sender, View.DragEventArgs e)
        {
            //To continue to receive drag events, including a possible drop event, a drag event listener must return true
            System.Diagnostics.Debug.WriteLine(e.Event.ToString());

        }

        const string EXERCISE_SETS_IDS_KEY = "exercise_sets_ids";

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            SaveExerciseSets();
            int[] ids = exercise_sets_adapter.ExerciseSets.Select(es => es.ID).ToArray();
            outState.PutIntArray(EXERCISE_SETS_IDS_KEY, ids);

            outState.PutInt("exercise_sets_locked", LockedSetCount);
        }

        void SaveExerciseSets()
        {
            // TODO: merge sets that are the same, add set count
            exercise_sets_adapter.RemoveZeroSets();

            foreach (IExerciseSets ex_sets in exercise_sets_adapter.ExerciseSets)
            {
                Database.InsertOrUpdateNoID<ExerciseSets>((ExerciseSets)ex_sets);
            }
        }

        private void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveExerciseSets();

                if(exercise_sets_adapter.Count == 0)
                {
                    Toast.MakeText(this, 
                        "You must have exercises in your routine. ",
                        ToastLength.Long).Show();
                    return;
                }

                Routine routine = new Routine(RoutineTitleText.Text, 
                    exercise_sets_adapter.ExerciseSets);
                routine.Database = Database;
                //Database.Insert(routine);
                Database.InsertOrUndeleteAndUpdate(routine);

                // if this routine is being edited, then delete the old one
                if(RoutineToDeleteIfDifferent != null &&
                    !RoutineToDeleteIfDifferent.Equals(routine))
                {
                    Database.HideDeletable<Routine>((Routine)RoutineToDeleteIfDifferent);
                }

                // set the category for all of the exercises in this routine
                foreach(ExerciseSets ex_sets in exercise_sets_adapter.ExerciseSets)
                {
                    Exercise ex = ex_sets.Exercise;
                    ex.Category = routine.Name;
                    Database.Update(ex);
                }

                ReturnRoutine(routine);
            }
            catch(ArgumentException ae)
            {
                Toast.MakeText(this,
                         ae.Message,
                        ToastLength.Long).Show();
            }
        }

        void ReturnRoutine(IRoutine routine)
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
            intent.PutExtra("routine_name", RoutineTitleText.Text);
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
                Exercise selected_exercise = Database.ReadByID<Exercise>(id);

                //exercise_adapter.Add(selected_exercise);
                //routine_exercises.Add(selected_exercise);

                // set the routine name from the category if user hasn't specified one yet
                if(String.IsNullOrWhiteSpace(RoutineTitleText.Text)
                    && !String.IsNullOrWhiteSpace(selected_exercise.Category))
                {
                    Toast.MakeText(this, "Routine title set to \"" + selected_exercise.Category +
                        "\" based on the added exercise's category", 
                        
                        ToastLength.Long).Show();
                    RoutineTitleText.Text = selected_exercise.Category;
                }

                ExerciseSets es = new ExerciseSets(selected_exercise, DEFAULT_SET_COUNT);
                es.Database = Database;
                exercise_sets_adapter.ExerciseSets.Add(es);
            }
        }
    }
}