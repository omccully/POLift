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
        protected readonly List<Binding> Bindings = new List<Binding>();

        protected TimerViewModel TimerVm
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

            System.Diagnostics.Debug.WriteLine(this.GetType() + " intent extras:");
            System.Diagnostics.Debug.WriteLine(Intent.Extras.Inspect());

            if(savedInstanceState != null)
            {
                System.Diagnostics.Debug.WriteLine(this.GetType() + " savedInstanceState:");
                System.Diagnostics.Debug.WriteLine(savedInstanceState.Inspect());
            }
            




            List<KeyValueStorage> storages = new List<KeyValueStorage>();
            if (savedInstanceState != null)
            {   // prefer saved state
                storages.Add(new BundleKeyValueStorage(savedInstanceState));
            }
            storages.Add(new BundleKeyValueStorage(Intent.Extras));

            TimerVm.RestoreState(new ChainedKeyValueStorage(storages));

            Bindings.Add(this.SetBinding(
               () => BaseVm.RoutineDetails,
               () => RoutineDetails.Text));

            Bindings.Add(this.SetBinding(
               () => BaseVm.WeightInputText,
               () => WeightEditText.Text,
               BindingMode.TwoWay));

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
              () => TimerVm.TimerIsStartable,
              () => ReportResultButton.Enabled));

            Bindings.Add(this.SetBinding(
               () => TimerVm.TimerState,
               () => this.TimerState));

            

            //TimerVm.PropertyChanged += TimerVm_PropertyChanged;

            BaseVm.DialogService = new DialogService(
                new DialogBuilderFactory(this),
                ViewModelLocator.Default.KeyValueStorage);

            TimerVm.Vibrator = new AndroidVibrator(this);

            ReportResultButton.Click += ReportResultButton_Click;

            Sub30SecButton.Click += Sub30SecButton_Click;
            Add30SecButton.Click += Add30SecButton_Click;
            SkipTimerButton.Click += SkipTimerButton_Click;

            parent_intent = (Intent)Intent.GetParcelableExtra("parent_intent");
            TimerVm.TimerFinishedNotificationService =
                new NotificationService(this, parent_intent, GetActivityState);

            LicenseManager = ViewModelLocator.Default.LicenseManager;

            Window.SetSoftInputMode(SoftInput.StateHidden);

            if (LicenseManager.ShowAds)
            {
                InitializeAds();
            }
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
                switch(value)
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

       /* private void TimerVm_PropertyChanged(object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "TimerIsStartable")
            {
                if(TimerVm.TimerIsStartable)
                {
                    CountDownTextView.SetTextColor(Android.Graphics.Color.Green);
                }
                else
                {
                    CountDownTextView.SetTextColor(Android.Graphics.Color.Orange);
                }
            }
        }*/

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

        Random randy = new Random();
        InterstitialAd mInterstitialAd;
        protected void TryShowFullScreenAd()
        {
            if (LicenseManager.ShowAds)
            {
                if(randy.Next(4) == 0)
                {
#if !DEBUG
                    mInterstitialAd.LoadAd(new AdRequest.Builder().Build());
#endif
                }
            }
        }

        protected abstract void ReportResultButton_Click(object sender, EventArgs e);

        protected Bundle GetActivityState()
        {
            Bundle bundle = new Bundle();
            SaveActivityState(bundle);
            return bundle;
        }

        protected void SaveActivityState(Bundle outState)
        {
            // this relies on the ViewModels entirely
            // if for some reason a child class decides to set 
            // its own state variables, it would cause problems

            BundleKeyValueStorage bkvs =
                 new BundleKeyValueStorage(outState);
            TimerVm.SaveState(bkvs);
            BaseVm.SaveState(bkvs);

            outState.PutParcelable("parent_intent", parent_intent);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            SaveActivityState(outState);
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

        protected virtual void RestoreActivityState(Bundle savedInstanceState)
        {
            Log.Debug("POLift", "PerformRoutineBaseActivity.RestoreActivityState");
            BundleKeyValueStorage bkvs = new BundleKeyValueStorage(savedInstanceState);
            TimerVm.RestoreState(bkvs);
            BaseVm.RestoreState(bkvs);
        }


        protected virtual void SkipTimerButton_Click(object sender, EventArgs e)
        {
            TimerVm.SkipTimer();
            //CountDownTextView.SetTextColor(Android.Graphics.Color.White);
        }

        /*protected static Bundle save_state;
        protected override void OnPause()
        {
            save_state = GetActivityState();
            base.OnPause();
        }

        protected override void OnResume()
        {
            if(save_state != null)
            {
                BundleKeyValueStorage bkvs = new BundleKeyValueStorage(save_state);
                TimerVm.RestoreState(bkvs);
                BaseVm.RestoreState(bkvs);
                save_state = null;
            }
            
            base.OnResume();
        }*/

        protected override void OnDestroy()
        {
            // dismiss dialog boxes to prevent window leaks
            BaseVm.DialogService.Dispose();

            foreach (Binding b in Bindings)
            {
                b.Detach();
            }

            base.OnDestroy();
        }
    }
}