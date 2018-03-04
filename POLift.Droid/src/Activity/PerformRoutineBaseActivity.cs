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
using Android.Gms.Ads;
using Android.Util;
using Android.Support.Compat;
using Android.Support.V4.App;

using Microsoft.Practices.Unity;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Service;
    using Core.Service;
    using Core.Model;
    using Core.ViewModel;

    //[Activity(Label = "Perform routine")]
    public abstract class PerformRoutineBaseActivity : Activity
    {
        // Keep track of bindings to avoid premature garbage collection
        protected readonly List<Binding> bindings = new List<Binding>();

        private TimerViewModel TimerVm
        {
            get
            {
                return ViewModelLocator.Default.Timer;
            }
        }

        protected abstract PerformBaseViewModel BaseVm { get; }

        protected TextView RoutineDetails;
        protected TextView NextExerciseView;

        protected TextView WeightLabel;
        protected EditText WeightEditText;
        protected TextView PlateMathTextView;

        protected TextView RepResultLabel;
        protected EditText RepResultEditText;
        protected TextView RepDetailsTextView;

        protected TextView NextWarmupView;

        protected Button ReportResultButton;

        protected TextView CountDownTextView;
        protected Button Sub30SecButton;
        protected Button Add30SecButton;
        protected Button SkipTimerButton;
        
        protected Button ModifyRestOfRoutineButton;

        protected Button IMadeAMistakeButton;
        protected Button EditThisExerciseButton;

        protected LinearLayout PerformRoutineMainContent;

        protected ILicenseManager LicenseManager;

        Intent parent_intent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Log.Debug("POLift", "PerformRoutineBaseActivity.OnCreate()");

            base.OnCreate(savedInstanceState);

            // Create your application here

            SetContentView(Resource.Layout.PerformRoutine);

            PerformRoutineMainContent = FindViewById<LinearLayout>(
                Resource.Id.PerformRoutineMainContent);
            RoutineDetails = FindViewById<TextView>(Resource.Id.RoutineDetails);
            NextExerciseView = FindViewById<TextView>(Resource.Id.NextExerciseView);
            ReportResultButton = FindViewById<Button>(Resource.Id.ReportResultButton);
            RepResultEditText = FindViewById<EditText>(Resource.Id.RepResultEditText);
            RepDetailsTextView = FindViewById<TextView>(Resource.Id.RepDetailsTextView);
            CountDownTextView = FindViewById<TextView>(Resource.Id.CountDownTextView);
            WeightEditText = FindViewById<EditText>(Resource.Id.WeightEditText);
            Sub30SecButton = FindViewById<Button>(Resource.Id.Sub30SecButton);
            Add30SecButton = FindViewById<Button>(Resource.Id.Add30SecButton);
            SkipTimerButton = FindViewById<Button>(Resource.Id.SkipTimerButton);
            PlateMathTextView = FindViewById<TextView>(Resource.Id.PlateMathTextView);
            RepResultLabel = FindViewById<TextView>(Resource.Id.RepResultLabel);
            WeightLabel = FindViewById<TextView>(Resource.Id.WeightLabel);
            ModifyRestOfRoutineButton = FindViewById<Button>(Resource.Id.ModifyRestOfRoutineButton);
            NextWarmupView = FindViewById<TextView>(Resource.Id.NextWarmupView);
            IMadeAMistakeButton = FindViewById<Button>(Resource.Id.IMadeAMistakeButton);
            EditThisExerciseButton = FindViewById<Button>(Resource.Id.EditThisExerciseButton);

            RestoreTimerState(savedInstanceState);

            bindings.Add(this.SetBinding(
               () => BaseVm.RoutineDetails,
               () => RoutineDetails.Text));

            bindings.Add(this.SetBinding(
               () => BaseVm.WeightInputText,
               () => WeightEditText.Text,
               BindingMode.TwoWay));

            bindings.Add(this.SetBinding(
               () => TimerVm.TimerStatus,
               () => CountDownTextView.Text));

            bindings.Add(this.SetBinding(
               () => TimerVm.Add30SecButtonText,
               () => Add30SecButton.Text));

            bindings.Add(this.SetBinding(
               () => TimerVm.Sub30SecButtonText,
               () => Sub30SecButton.Text));

            bindings.Add(this.SetBinding(
              () => TimerVm.Add30SecEnabled,
              () => Add30SecButton.Enabled));

            bindings.Add(this.SetBinding(
              () => TimerVm.Sub30SecEnabled,
              () => Sub30SecButton.Enabled));

            bindings.Add(this.SetBinding(
             () => TimerVm.SkipTimerEnabled,
             () => SkipTimerButton.Enabled));

            bindings.Add(this.SetBinding(
              () => TimerVm.TimerIsStartable,
              () => ReportResultButton.Enabled));


            BaseVm.DialogService = new DialogService(
                new DialogBuilderFactory(this),
                ViewModelLocator.Default.KeyValueStorage);

            ReportResultButton.Click += ReportResultButton_Click;

            Sub30SecButton.Click += Sub30SecButton_Click;
            Add30SecButton.Click += Add30SecButton_Click;
            SkipTimerButton.Click += SkipTimerButton_Click;

            Intent parent_intent = (Intent)Intent.GetParcelableExtra("parent_intent");
            TimerVm.TimerFinishedNotificationService =
                new NotificationService(this, parent_intent, GetTimerState);

            LicenseManager = ViewModelLocator.Default.LicenseManager;

            Window.SetSoftInputMode(SoftInput.StateHidden);

            if (LicenseManager.ShowAds)
            {
                InitializeAds();
            }
        }

        void InitializeAds()
        {
            AdView ad_view = FindViewById<AdView>(Resource.Id.adView);

            ad_view.Visibility = ViewStates.Visible;
            var adRequest = new AdRequest.Builder()
#if DEBUG
                .AddTestDevice("9B7495553B1FEB2545BD24C542420C7F")
#else
#endif
                .Build();

            ad_view.LoadAd(adRequest);

            EventDrivenAdListener ad_listener = new EventDrivenAdListener();
            ad_listener.AdLoaded += Ad_listener_AdLoaded;
            ad_listener.AdClosed += Ad_listener_AdClosed;

            mInterstitialAd = new InterstitialAd(this);
            mInterstitialAd.AdListener = ad_listener;

#if DEBUG
            mInterstitialAd.AdUnitId = Resources.GetString(Resource.String.interstitial_ad_unit_id_test);
#else
            mInterstitialAd.AdUnitId = Resources.GetString(Resource.String.interstitial_ad_unit_id);
#endif
        }

        private void Add30SecButton_Click(object sender, EventArgs e)
        {
            TimerVm.Add30Sec();
        }

        private void Sub30SecButton_Click(object sender, EventArgs e)
        {
            TimerVm.Sub30Sec();
        }

        private void Ad_listener_AdClosed(object sender, EventArgs e)
        {
            Toast.MakeText(this, Resource.String.consider_purchase, 
                ToastLength.Long).Show();
        }

        private void Ad_listener_AdLoaded(object sender, EventArgs e)
        {
            mInterstitialAd.Show();
        }

        InterstitialAd mInterstitialAd;
        protected void TryShowFullScreenAd()
        {
            if (LicenseManager.ShowAds)
            {
                mInterstitialAd.LoadAd(new AdRequest.Builder().Build());
            }
        }

        protected abstract void ReportResultButton_Click(object sender, EventArgs e);

        Bundle GetTimerState()
        {
            Bundle bundle = new Bundle();

            BundleKeyValueStorage bkvs = 
                new BundleKeyValueStorage(bundle);
            TimerVm.SaveState(bkvs);

            return bundle;
        }

        protected virtual void SaveTimerState(Bundle outState)
        {
            BundleKeyValueStorage bkvs =
                 new BundleKeyValueStorage(outState);
            TimerVm.SaveState(bkvs);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            SaveTimerState(outState);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            Log.Debug("POLift", "PerformRoutineBaseActivity.OnRestoreInstanceState()");

            RestoreTimerState(savedInstanceState);

            base.OnRestoreInstanceState(savedInstanceState);
        }

        protected virtual void RestoreTimerState(Bundle savedInstanceState)
        {
            TimerVm.RestoreState(
                new BundleKeyValueStorage(savedInstanceState));
        }

        protected virtual void SkipTimerButton_Click(object sender, EventArgs e)
        {
            TimerVm.SkipTimer();
            CountDownTextView.SetTextColor(Android.Graphics.Color.White);
        }
        
        protected virtual void SaveStateToIntent(Intent intent)
        {
            intent.PutExtras(GetTimerState());
        }
    }
}