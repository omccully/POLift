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
using Android.Preferences;

using GalaSoft.MvvmLight.Helpers;


using POLift.Core.ViewModel;
using POLift.Core.Model;
using POLift.Core.Service;

namespace POLift.Droid
{
    using Service;

    [Activity(Label = "Perform routine (list view)", 
        ParentActivity = typeof(MainActivity))]
    public class PerformRoutineListActivity : Activity
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> Bindings = new List<Binding>();

        const int WarmUpRoutineRequestCode = 100;
        const int EditRoutineResultRequestCode = 3134;
        const int SelectExerciseRequestCode = 452;

        PerformRoutineListViewModel Vm
        {
            get
            {
                return null;
            }
        }

        TimerViewModel TimerVm
        {
            get
            {
                return (TimerViewModel)Vm.TimerViewModel;
            }
        }

        TextView ExerciseDetailsTextView;

        TextView CountDownTextView;
        Button Sub30SecButton;
        Button SkipTimerButton;
        Button Add30SecButton;

        ListView ExercisesListView;

        ISharedPreferences prefs;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PerformRoutineList);

            ExerciseDetailsTextView = FindViewById<TextView>(Resource.Id.ExerciseDetailsTextView);
            Sub30SecButton = FindViewById<Button>(Resource.Id.Sub30SecButton);
            SkipTimerButton = FindViewById<Button>(Resource.Id.SkipTimerButton);
            Add30SecButton = FindViewById<Button>(Resource.Id.Add30SecButton);
            ExercisesListView = FindViewById<ListView>(Resource.Id.ExercisesListView);




            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            List<KeyValueStorage> storages = new List<KeyValueStorage>();
            if (savedInstanceState != null)
            {   // prefer saved state
                storages.Add(new BundleKeyValueStorage(savedInstanceState));
            }
            storages.Add(new BundleKeyValueStorage(Intent.Extras));
            storages.Add(TimerVm.KeyValueStorage);

            TimerVm.RestoreState(new ChainedKeyValueStorage(storages));

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this),
                ViewModelLocator.Default.KeyValueStorage);

            TimerVm.Vibrator = new AndroidVibrator(this);


            Bindings.Add(this.SetBinding(
                () => Vm.ExerciseDetails,
                () => ExerciseDetailsTextView.Text));

            Bindings.Add(this.SetBinding(
               () => TimerVm.TimerStatus,
               () => CountDownTextView.Text));

            Bindings.Add(this.SetBinding(
               () => TimerVm.Add30SecButtonText,
               () => Add30SecButton.Text));

            Bindings.Add(this.SetBinding(
               () => TimerVm.Sub30SecButtonText,
               () => Sub30SecButton.Text));

            Bindings.Add(this.SetBinding(
              () => TimerVm.Add30SecEnabled,
              () => Add30SecButton.Enabled));

            Bindings.Add(this.SetBinding(
              () => TimerVm.Sub30SecEnabled,
              () => Sub30SecButton.Enabled));

            Bindings.Add(this.SetBinding(
             () => TimerVm.SkipTimerEnabled,
             () => SkipTimerButton.Enabled));

            Bindings.Add(this.SetBinding(
               () => TimerVm.TimerState,
               () => this.TimerState));

            Add30SecButton.SetCommand(TimerVm.Add30SecCommand);
            Sub30SecButton.SetCommand(TimerVm.Sub30SecCommand);
            SkipTimerButton.SetCommand(TimerVm.SkipTimerCommand);

            Vm.Toaster = new Toaster(this);
            Vm.StartWarmup = StartWarmupActivity;

            Vm.NavigateSelectExercise = NavigateSelectExercise;

            object SavedState = null;
            if (SavedState != null)
            {
                //RestoreActivityState(SavedState);
                SavedState = null;
            }
            else
            {
                storages = new List<KeyValueStorage>();
                if (savedInstanceState != null)
                    storages.Add(new BundleKeyValueStorage(savedInstanceState));
                storages.Add(new BundleKeyValueStorage(Intent.Extras));

                Vm.RestoreState(new ChainedKeyValueStorage(storages));
                //Log.Debug("POLift", "Vm.RestoreState from PerformRoutineActivity.OnCreate");
            }
        }

        void NavigateSelectExercise()
        {
            var intent = new Intent(this, typeof(SelectExerciseActivity));
            intent.PutExtra("routine_name", Vm.Routine.Name);
            StartActivityForResult(intent, SelectExerciseRequestCode);
        }

        void StartWarmupActivity()
        {
            System.Diagnostics.Debug.WriteLine("StartWarmupActivity --------------------");
            var intent = new Intent(this, typeof(WarmupRoutineActivity));
            //intent.PutExtra("exercise_id", Vm.CurrentExercise.ID);
            //intent.PutExtra("working_set_weight", Vm.WeightInputText);

            PerformWarmupViewModel.InitializationState(new IntentKeyValueStorage(intent),
                Vm.CurrentExercise, Vm.WeightInputText);
            intent.PutExtra("parent_intent", this.Intent);

            WarmupRoutineActivity.SavedState = null;

            StartActivityForResult(intent, WarmUpRoutineRequestCode);
        }

        TimerState _TimerState = TimerState.Skipped;
        public TimerState TimerState
        {
            get
            {
                return _TimerState;
            }
            set
            {
                switch (value)
                {
                    case TimerState.Skipped:
                        CountDownTextView.SetTextColor(Android.Graphics.Color.White);
                        break;
                    case TimerState.RunningPositive:
                        CountDownTextView.SetTextColor(Android.Graphics.Color.Orange);
                        break;
                    case TimerState.Elapsed:
                        CountDownTextView.SetTextColor(Android.Graphics.Color.Green);
                        break;
                }

                _TimerState = value;
            }
        }
    }
}