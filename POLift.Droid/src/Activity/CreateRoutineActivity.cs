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

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Core.Model;
    using Core.Service;
    using Adapter;
    using Core.ViewModel;
    using Service;

    [Activity(Label = "Create Routine", WindowSoftInputMode = SoftInput.AdjustPan)]
    class CreateRoutineActivity : Activity
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private CreateRoutineViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.CreateRoutine;
            }
        }

        EditText RoutineTitleText;
        ListView ExercisesListView;
        Button AddExerciseButton;
        Button CreateRoutineButton;

        ExerciseSetsAdapter exercise_sets_adapter;

        const int SelectExerciseRequestCode = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("CreateRoutineActivity.OnCreate()");

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CreateRoutine);

            RoutineTitleText = FindViewById<EditText>(Resource.Id.RoutineTitleText);
            ExercisesListView = FindViewById<ListView>(Resource.Id.ExercisesListView);
            AddExerciseButton = FindViewById<Button>(Resource.Id.AddExerciseButton);
            CreateRoutineButton = FindViewById<Button>(Resource.Id.CreateRoutineButton);

            bindings.Add(this.SetBinding(
               () => Vm.RoutineNameInput,
               () => RoutineTitleText.Text,
               BindingMode.TwoWay));

            Vm.Reset();

            int edit_routine_id = Intent.GetIntExtra("edit_routine_id", -1);

            if (savedInstanceState != null)
            {
                Vm.RestoreState(new BundleKeyValueStorage(savedInstanceState));
            }
            else if (edit_routine_id != -1)
            {
                // edit routine
                // sets LockedSets, LockedExerciseSets, RoutineToDeleteIfDifferent
                Vm.EditRoutine(edit_routine_id, Intent.GetIntExtra("exercises_locked", 0));
            }

            InitializeExerciseSetsAdapter();

            AddExerciseButton.Click += AddExerciseButton_Click;
            CreateRoutineButton.Click += CreateRoutineButton_Click;
        }

        void InitializeExerciseSetsAdapter()
        {
            exercise_sets_adapter = new ExerciseSetsAdapter(this,
                Vm.ExerciseSets, Vm.LockedExerciseSets);

            ExercisesListView.Adapter = exercise_sets_adapter;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            Vm.SaveState(new BundleKeyValueStorage(outState));
        }

        private void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            Routine r = Vm.CreateRoutineFromInput();
            if (r != null) ReturnRoutine(r);
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
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                int id = data.GetIntExtra("exercise_id", -1);
                if (id == -1) return;

                Vm.SelectExercise_ValueChosen(id);
            }
        }

        protected override void OnDestroy()
        {
            exercise_sets_adapter?.Dispose();

            base.OnDestroy();
        }
    }
}