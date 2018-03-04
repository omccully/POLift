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
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

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

            bindings.Add(this.SetBinding(
               () => BaseVm.ExerciseDetails,
               () => NextExerciseView.Text));

            bindings.Add(this.SetBinding(
               () => BaseVm.PlateMathDetails,
               () => PlateMathTextView.Text));

            bindings.Add(this.SetBinding(
               () => Vm.RepsInputText,
               () => RepResultEditText.Text,
               BindingMode.TwoWay));

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

        protected override void OnSaveInstanceState(Bundle outState)
        {
            Vm.SaveState(new BundleKeyValueStorage(outState));
            
            base.OnSaveInstanceState(outState);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            System.Diagnostics.Debug.WriteLine("PerformRoutineActivity.OnActivityResult()");
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok)
            {
                if (requestCode == WarmUpRoutineRequestCode)
                {
                    if (Vm.CurrentExercise != null)
                    {
                        // start timer once warmup routine is finished.
                        Vm.StartTimer();
                    }
                }
                else if (requestCode == ModifyRestOfRoutineResultCode)
                {
                    int id = data.GetIntExtra("routine_id", -1);
                    if (id == -1) return;

                    Vm.CreateRoutineViewModel_ValueChosen(id);
                }
                else if(requestCode == EditRoutineResultRequestCode)
                {
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
            Vm.SubmitResultFromInput();
        }

        void PromptUserForRating()
        {
            Vm.PromptUserForRating(delegate
            {
                try
                {
                    StartActivity(new Intent(Intent.ActionView,
                        Android.Net.Uri.Parse("market://details?id=com.cml.polift")));
                }
                catch { }
            });
        }


        void StartWarmupActivity()
        {
            System.Diagnostics.Debug.WriteLine("StartWarmupActivity");
            var intent = new Intent(this, typeof(WarmupRoutineActivity));
            intent.PutExtra("exercise_id", Vm.CurrentExercise.ID);
            intent.PutExtra("working_set_weight", Vm.WeightInputText);
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

        protected override void SaveStateToIntent(Intent intent)
        {
            base.SaveStateToIntent(intent);

            Vm.SaveState(new BundleKeyValueStorage(intent.Extras));
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
 