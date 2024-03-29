﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using System.Diagnostics;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Service;
    using Android.Text;
    using Core.Model;
    using Core.Service;
    using Core.ViewModel;

    [Activity(Label = "Warmup", ParentActivity = typeof(PerformRoutineActivity))]
    public class WarmupRoutineActivity : PerformRoutineBaseActivity
    {
        private PerformWarmupViewModel Vm
        {
            get => ViewModelLocator.Default.PerformWarmup;
        }

        protected override PerformBaseViewModel BaseVm
        {
            get => Vm;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Log.Debug("POLift", "**************************************");
            Log.Debug("POLift", "WarmupRoutineActivity.OnCreate()");

            AndroidHelpers.SetActivityDepth(this, 2);

            base.OnCreate(savedInstanceState);

            Bindings.Add(this.SetBinding(
              () => BaseVm.ExerciseDetails,
              () => NextWarmupView.Text));

            if (SavedState != null)
            {
                RestoreActivityState(SavedState);
                SavedState = null;
            }
            else
            {
                List<KeyValueStorage> storages = new List<KeyValueStorage>();
                if (savedInstanceState != null)
                {   // prefer saved state
                    storages.Add(new BundleKeyValueStorage(savedInstanceState));
                }
                storages.Add(new BundleKeyValueStorage(Intent.Extras));

                Vm.RestoreState(new ChainedKeyValueStorage(storages));
                Log.Debug("POLift", "Vm.RestoreState from WarmupRoutineActivity.OnCreate");
            }

            //RoutineDetails.Visibility = ViewStates.Gone;
            WeightLabel.Text = "Working set weight: ";
            ReportResultButton.Text = "Set completed";
            RepResultLabel.Visibility = ViewStates.Gone;
            RepResultEditText.Visibility = ViewStates.Gone;
            ModifyRestOfRoutineButton.Visibility = ViewStates.Gone;
            NextExerciseView.Visibility = ViewStates.Gone;

            IMadeAMistakeButton.Text = "Skip warmup routine";
            IMadeAMistakeButton.Click += IMadeAMistakeButton_Click;

            EditThisExerciseButton.Visibility = ViewStates.Gone;

            RepDetailsTextView.Visibility = ViewStates.Gone;

            if (Vm.WarmupFinished)
            {
                SetResult(Result.Ok);
                Finish();
            }

            Log.Debug("POLift", "Warmup final " + sw.ElapsedMilliseconds + "ms");
        }

        private void IMadeAMistakeButton_Click(object sender, EventArgs e)
        {
            Vm.SkipWarmup(delegate
            {
                SetResult(Result.Canceled);
                Finish();
            });
        }

        public override void OnBackPressed()
        {
            Vm.SkipWarmup(delegate
            {
                base.OnBackPressed();
            });
        }

        public const string NewWorkingWeightKey = "new_working_weight";
        protected override void ReportResultButton_Click(object sender, EventArgs e)
        {
            bool finished = Vm.WarmupSetFinished(delegate
            { 
                // warmup finished action
                Intent result_intent = new Intent();

                float weight;
                if (Single.TryParse(Vm.WeightInputText, out weight))
                {
                    result_intent.PutExtra(NewWorkingWeightKey, weight);
                }
               
                SetResult(Result.Ok, result_intent);
                Finish();
            });

            if (!finished)
            {
                TryShowFullScreenAd();
            }
        }

        public static Bundle SavedState;
        protected override void OnPause()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnPause()");

            SavedState = GetActivityState();

            base.OnPause();
        }

        protected override void OnResume()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnResume()");
            if (SavedState != null)
            {
                RestoreActivityState(SavedState);
                SavedState = null;
            }

            base.OnResume();
        }

        protected override void OnStop()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnStop()");

            base.OnStop();
        }

        protected override void OnDestroy()
        {
            Log.Debug("POLift", "WarmupRoutineActivity.OnDestroy()");

            base.OnDestroy();
        }
    }
}