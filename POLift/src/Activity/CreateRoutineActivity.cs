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
    using Core.Model;
    using Core.Service;
    using Adapter;

    [Activity(Label = "Create Routine", WindowSoftInputMode = SoftInput.AdjustPan)]
    class CreateRoutineActivity : Activity
    {
        EditText RoutineTitleText;
        ListView ExercisesListView;
        Button AddExerciseButton;
        Button CreateRoutineButton;

        ExerciseSetsAdapter exercise_sets_adapter;

        const string RoutineIDToDeleteIfDifferentKey = "routine_id_to_delete_if_different";
        static int SelectExerciseRequestCode = 1;

        IRoutine RoutineToDeleteIfDifferent = null;

        int ExercisesLocked = 0;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("CreateRoutineActivity.OnCreate()");

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreateRoutine);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutineTitleText = FindViewById<EditText>(Resource.Id.RoutineTitleText);
            ExercisesListView = FindViewById<ListView>(Resource.Id.ExercisesListView);
            AddExerciseButton = FindViewById<Button>(Resource.Id.AddExerciseButton);
            CreateRoutineButton = FindViewById<Button>(Resource.Id.CreateRoutineButton);

            int edit_routine_id = Intent.GetIntExtra("edit_routine_id", -1);

            if (savedInstanceState != null)
            {
                int[] ids = savedInstanceState.GetIntArray(EXERCISE_SETS_IDS_KEY);
                ExercisesLocked = savedInstanceState.GetInt("exercises_locked", 0);
                if (ids != null)
                {
                    System.Diagnostics.Debug.WriteLine("ids after save: " + ids.ToIDString());
                    //foreach (int id in ids) System.Diagnostics.Debug.WriteLine("esid = " + id);
                    List<IExerciseSets> exercise_sets = 
                        new List<IExerciseSets>(Database.ParseIDs<ExerciseSets>(ids));

                    InitializeExerciseSetsAdapter(exercise_sets, ExercisesLocked);
                }

                int RoutineIDToDeleteIfDifferent = savedInstanceState
                    .GetInt(RoutineIDToDeleteIfDifferentKey, 0);
                if (RoutineIDToDeleteIfDifferent > 0)
                {
                    RoutineToDeleteIfDifferent = 
                        Database.ReadByID<Routine>(RoutineIDToDeleteIfDifferent);
                }
            }
            else if (edit_routine_id != -1)
            {
                // edit routine
                Routine routine = Database.ReadByID<Routine>(edit_routine_id);

                RoutineTitleText.Text = routine.Name;

                // take the exercises list, split it at location exercises_locked
                // ExerciseSets.Group(Exercise[] exercises)

                ExercisesLocked = Intent.GetIntExtra("exercises_locked", 0);

                InitializeExerciseSetsAdapter(routine.Exercises, ExercisesLocked);

                RoutineToDeleteIfDifferent = routine;
            }
            // else, start new routine

            // initialize the adapter if it hasn't been so far..
            if (exercise_sets_adapter == null)
            {
                exercise_sets_adapter = new ExerciseSetsAdapter(this, 
                    new List<IExerciseSets>() { });
            }
            

            ExercisesListView.Adapter = exercise_sets_adapter;


            AddExerciseButton.Click += AddExerciseButton_Click;
            CreateRoutineButton.Click += CreateRoutineButton_Click;
        }

        void InitializeExerciseSetsAdapter(IEnumerable<IExerciseSets> exercise_sets, 
            int exercises_locked)
        {
            InitializeExerciseSetsAdapter(ExerciseSets.Expand(exercise_sets), exercises_locked);
        }

        void InitializeExerciseSetsAdapter(IEnumerable<IExercise> exercises, 
            int exercises_locked)
        {
            List<IExerciseSets> exercise_sets = new List<IExerciseSets>();

            IEnumerable<IExerciseSets> locked_sets =
                    ExerciseSets.Group(exercises.Take(exercises_locked), Database);
            IEnumerable<IExerciseSets> unlocked_sets =
                ExerciseSets.Group(exercises.Skip(exercises_locked), Database);
            int locked_set_count = locked_sets.Count();
            exercise_sets.AddRange(locked_sets);
            exercise_sets.AddRange(unlocked_sets);

            exercise_sets_adapter = new ExerciseSetsAdapter(this,
                exercise_sets, locked_set_count);
        }

        const string EXERCISE_SETS_IDS_KEY = "exercise_sets_ids";

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            
            SaveExerciseSets();
            int[] ids = exercise_sets_adapter.ExerciseSets.Select(es => es.ID).ToArray();
            System.Diagnostics.Debug.WriteLine("ids during save: " + ids.ToIDString());

            outState.PutIntArray(EXERCISE_SETS_IDS_KEY, ids);

            //System.Diagnostics.Debug.WriteLine("saving exercise_sets_locked=" + LockedSetCount);
            //outState.PutInt("exercise_sets_locked", LockedSetCount);
            outState.PutInt("exercises_locked", ExercisesLocked);


            if (RoutineToDeleteIfDifferent != null)
            {
                outState.PutInt(RoutineIDToDeleteIfDifferentKey,
                    RoutineToDeleteIfDifferent.ID);
            }
        }

        void SaveExerciseSets()
        {
            exercise_sets_adapter.RemoveZeroSets();
            exercise_sets_adapter.Regroup(Database);

            foreach (IExerciseSets ex_sets in exercise_sets_adapter.ExerciseSets)
            {
                Database.InsertOrUpdateNoID<ExerciseSets>((ExerciseSets)ex_sets);
            }
        }

        private void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            try
            {
                if(exercise_sets_adapter.Count == 0)
                {
                    Toast.MakeText(this, 
                        "You must have exercises in your routine. ",
                        ToastLength.Long).Show();
                    return;
                }

                SaveExerciseSets();

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