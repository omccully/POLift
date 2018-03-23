using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;
using Android.Util;

using System.Diagnostics;

using Microsoft.Practices.Unity;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Service;

    using Core.Service;
    using Core.Model;
    using Core.ViewModel;

    [Activity(Label = "Perform routine", ParentActivity = typeof(MainActivity))]
    public class PerformRoutineActivity : PerformRoutineBaseActivity
    {
        private PerformRoutineViewModel Vm
        {
            get => ViewModelLocator.Default.PerformRoutine;
        }

        protected override PerformBaseViewModel BaseVm
        {
            get => Vm;
        }

        const int WarmUpRoutineRequestCode = 100;
        const int EditRoutineResultRequestCode = 991234;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int previous_activity_depth = AndroidHelpers.GetActivityDepth(this);
            AndroidHelpers.SetActivityDepth(this, 1);

            System.Diagnostics.Debug.WriteLine("PerformRoutineActivity.OnCreate()");
            
            base.OnCreate(savedInstanceState);

            IMadeAMistakeButton.Click += IMadeAMistakeButton_Click;

            NextWarmupView.Visibility = ViewStates.Gone;

            ModifyRestOfRoutineButton.Click += ModifyRestOfRoutineButton_Click;

            EditThisExerciseButton.Click += EditThisExerciseButton_Click;

            Vm.ResultSubmittedWithoutCompleting += Vm_ResultSubmittedWithoutCompleting;

            Bindings.Add(this.SetBinding(
               () => BaseVm.ExerciseDetails,
               () => NextExerciseView.Text));

            Bindings.Add(this.SetBinding(
               () => BaseVm.PlateMathDetails,
               () => PlateMathTextView.Text));

            Bindings.Add(this.SetBinding(
               () => Vm.RepDetails,
               () => RepDetailsTextView.Text));

            Bindings.Add(this.SetBinding(
               () => Vm.RepsInputText,
               () => RepResultEditText.Text,
               BindingMode.TwoWay));

            Bindings.Add(this.SetBinding(
               () => TimerVm.TimerIsStartable,
               () => RepResultEditText.Enabled));

            Vm.Toaster = new Toaster(this);
            Vm.StartWarmup = StartWarmupActivity;

            List<KeyValueStorage> storages = new List<KeyValueStorage>();
            if (savedInstanceState != null)
                storages.Add(new BundleKeyValueStorage(savedInstanceState));
            storages.Add(new BundleKeyValueStorage(Intent.Extras));

            Vm.RestoreState(new ChainedKeyValueStorage(storages));

            //Log.Debug("POLift", "perform finish " + sw.ElapsedMilliseconds + "ms");
        }

        private void Vm_ResultSubmittedWithoutCompleting(object sender, EventArgs e)
        {
            TryShowFullScreenAd();
            PromptUserForRating();
        }

        const int EditExerciseRequestCode = 9851;
        private void EditThisExerciseButton_Click(object sender, EventArgs e)
        {
            if (Vm.Routine == null) return;

            Intent intent = new Intent(this, typeof(CreateExerciseActivity));
            intent.PutExtra("edit_exercise_id", Vm.CurrentExercise.ID);
            StartActivityForResult(intent, EditExerciseRequestCode);
        }

        private void IMadeAMistakeButton_Click(object sender, EventArgs e)
        {
            Vm.IMadeAMistake(delegate
            {
                Intent intent = new Intent(this, typeof(EditRoutineResultActivity));
                intent.PutExtra("routine_result_id", Vm.RoutineResult.ID);
                StartActivityForResult(intent, EditRoutineResultRequestCode);
            });
        }

        const int ModifyRestOfRoutineResultCode = 6000;

        private void ModifyRestOfRoutineButton_Click(object sender, EventArgs e)
        {
            Intent result_intent = new Intent(this, typeof(CreateRoutineActivity));
            result_intent.PutExtra("edit_routine_id", Vm.Routine.ID);
            result_intent.PutExtra(
                CreateRoutineViewModel.ExercisesLockedKey, 
                Vm.RoutineResult.ResultCount);

            

            StartActivityForResult(result_intent, ModifyRestOfRoutineResultCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            System.Diagnostics.Debug.WriteLine("PerformRoutineActivity.OnActivityResult()");
            base.OnActivityResult(requestCode, resultCode, data);

            base.save_state = null;

            if(resultCode == Result.Ok)
            {
                if (requestCode == WarmUpRoutineRequestCode)
                {
                    float new_weight = data.GetFloatExtra(
                        WarmupRoutineActivity.NewWorkingWeightKey, 
                        Single.MinValue);
                    if (new_weight == Single.MinValue) return;

                    Vm.PerformWarmupViewModel_ValueChosen(new_weight);
                }
                else if (requestCode == ModifyRestOfRoutineResultCode)
                {
                    int id = data.GetIntExtra("routine_id", -1);
                    if (id == -1) return;

                    Vm.CreateRoutineViewModel_ValueChosen(id);
                }
                else if(requestCode == EditRoutineResultRequestCode)
                {
                    System.Diagnostics.Debug.WriteLine("EditRoutineResultRequestCode");

                    // set ER to null so next they're reloaded
                    Vm.RoutineResult.ExerciseResults = null;
                    Vm.RefreshRoutineDetails();
                }
                else if(requestCode == EditExerciseRequestCode)
                {
                    Log.Debug("POLift", "(requestCode == EditExerciseRequestCode)");
                    int id = data.GetIntExtra("exercise_id", -1);
                    if (id == -1) return;

                    Vm.CreateExercise_ValueChosen(id);
                }
            }
        }

        protected override void ReportResultButton_Click(object sender, EventArgs e)
        {
            // user submitted a result for this CurrentExercise
            Vm.SubmitResultFromInput(delegate
            {
                SetResult(Result.Ok);
                Finish();
            });
        }

        void PromptUserForRating()
        {
            Vm.PromptUserForRating(delegate
            {
                AndroidHelpers.NavigateToAppRating(this);
            });
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

            StartActivityForResult(intent, WarmUpRoutineRequestCode);
        }

        void ReturnRoutineResult(IRoutineResult routine_result)
        {
            ReturnRoutineResult(routine_result.ID);
        }

        void ReturnRoutineResult(int ID)
        {
            var result_intent = new Intent();
            result_intent.PutExtra("routine_result_id", ID);
            SetResult(Result.Ok, result_intent);
            Finish();
        }

        protected override void OnPause()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnPause()");

            base.OnPause();
        }

        protected override void OnStop()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnStop()");

            base.OnStop();
        }

        protected override void OnDestroy()
        {
            Log.Debug("POLift", "PerformWarmupActivity.OnDestroy()");

            base.OnDestroy();
        }
    }
}
 