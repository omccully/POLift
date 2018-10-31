using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Preferences;
using Android.Views.InputMethods;
using Android.Text;
using Android.Text.Method;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Core.ViewModel;
    using Core.Model;
    using Core.Service;
    using Service;

    [Activity(Label = "Create Exercise")]
    public class CreateExerciseActivity : Activity
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        EditText ExerciseNameText;
        EditText RepRangeMaxText;
        Button CreateExerciseButton;
        EditText RestPeriodSecondsText;
        EditText RestPeriodMinutesText;
        EditText WeightIncrementText;
        Spinner SelectMathTypeSpinner;
        TextView ExerciseDetailsTextView;
        EditText ConsecutiveSetsForWeightIncrease;

        IPOLDatabase Database;

        private CreateExerciseViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.CreateExercise;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.CreateExercise);

            Database = C.Database;

            ExerciseNameText = FindViewById<EditText>(Resource.Id.ExerciseNameText);
            RepRangeMaxText = FindViewById<EditText>(Resource.Id.RepRangeMaxText);
            CreateExerciseButton = FindViewById<Button>(Resource.Id.CreateExerciseButton);
            RestPeriodSecondsText = FindViewById<EditText>(Resource.Id.RestPeriodSecondsTextBox);
            RestPeriodMinutesText = FindViewById<EditText>(Resource.Id.RestPeriodMinutesTextBox);
            WeightIncrementText = FindViewById<EditText>(Resource.Id.WeightIncrementTextBox);
            SelectMathTypeSpinner = FindViewById<Spinner>(Resource.Id.SelectMathTypeSpinner);
            ExerciseDetailsTextView = FindViewById<TextView>(Resource.Id.ExerciseDetailsTextView);
            ConsecutiveSetsForWeightIncrease = FindViewById<EditText>(Resource.Id.ConsecutiveSetsForWeightIncrease);
            //RestPeriodSecondsText.InputType = Android.Text.InputTypes.DatetimeVariationTime;

            SelectMathTypeSpinner.Adapter = new PlateMathTypeAdapter(this, PlateMath.PlateMathTypes);
            //RestPeriodSecondsText.SetFilters(new IInputFilter[] { new TimeSpanInputFilter() });
            //RestPeriodSecondsText.InputType = InputTypes.ClassText;
            // RestPeriodSecondsText.Key
            RestPeriodSecondsText.FocusChange += RestPeriodSecondsText_FocusChange;
            RestPeriodMinutesText.FocusChange += RestPeriodMinutesText_FocusChange;
            SelectMathTypeSpinner.ItemSelected += SelectMathTypeSpinner_ItemSelected;

            ExerciseNameText.TextChanged += (s, e) => { };

            bindings.Add(this.SetBinding(
                () => Vm.SubmitButtonText,
                () => CreateExerciseButton.Text));

            bindings.Add(this.SetBinding(
               () => Vm.ExerciseNameInput,
               () => ExerciseNameText.Text,
               BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
               () => Vm.NameInputEnabled,
               () => ExerciseNameText.Enabled));

            bindings.Add(this.SetBinding(
              () => Vm.RepCountInput,
              () => RepRangeMaxText.Text,
              BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
              () => Vm.WeightIncrementInput,
              () => WeightIncrementText.Text,
              BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
                () => Vm.RestPeriodSecondsInput,
                () => RestPeriodSecondsText.Text,
                BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
               () => Vm.RestPeriodMinutesInput,
               () => RestPeriodMinutesText.Text,
               BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
               () => Vm.ConsecutiveSetsInput,
               () => ConsecutiveSetsForWeightIncrease.Text,
               BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
               () => Vm.ExerciseDetails,
               () => ExerciseDetailsTextView.Text,
               BindingMode.TwoWay));

            Vm.Toaster = new Toaster(this);

            int edit_exercise_id = Intent.GetIntExtra("edit_exercise_id", -1);
            if(edit_exercise_id == -1)
            {
                Vm.Reset();
            }
            else
            {
                bool allow_name_edit = Intent.GetBooleanExtra("allow_name_edit", true);

                IExercise exercise = Database.ReadByID<Exercise>(edit_exercise_id);

                Vm.EditExercise(exercise, allow_name_edit);

                SelectMathTypeSpinner.SetSelection(exercise.PlateMathID);
            }

            // TODO: fix this to make name text box focused when activity starts
            ExerciseNameText.RequestFocus();
            InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
            imm.ShowSoftInput(ExerciseNameText, ShowFlags.Implicit);

            CreateExerciseButton.Click += CreateExerciseButton_Click;

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this),
                ViewModelLocator.Default.KeyValueStorage);

            Vm.InfoUser();
        }

        private void RestPeriodMinutesText_FocusChange(object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            if (!e.HasFocus)
            {
                Vm.NormalizeRestPeriodMinutes();
            }
        }

        private void RestPeriodSecondsText_FocusChange(object sender, Android.Views.View.FocusChangeEventArgs e)
        {
            if(!e.HasFocus)
            {
                Vm.NormalizeRestPeriodSeconds();
            }
        }

        private void SelectMathTypeSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Vm.PlateMathID = e.Position;
        }

        protected void CreateExerciseButton_Click(object sender, EventArgs e)
        {
            Exercise result = Vm.CreateExerciseFromInput();

            if(result != null)
            {
                ReturnExercise(result);
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

        protected override void OnDestroy()
        {
            foreach(Binding binding in bindings)
            {
                binding.Detach();
            }

            base.OnDestroy();
        }
    }
}

