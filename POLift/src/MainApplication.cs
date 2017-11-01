using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Provider;
using Android.Gms.Ads;
using Android.Preferences;
using Plugin.CurrentActivity;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

using Microsoft.Practices.Unity;


namespace POLift
{
    using Service;

    //You can specify additional application information in this attribute
#if DEBUG
    [Application(Debuggable=true)]
#else
    [Application(Debuggable =false)]
#endif
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        Handler handler;
        ILicenseManager license_manager;

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
            System.Diagnostics.Debug.WriteLine("MainApplication(IntPtr handle, JniHandleOwnership transer)");
        }

        public override void OnCreate()
        {
            System.Diagnostics.Debug.WriteLine("OnCreate()");
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
            //A great place to initialize Xamarin.Insights and Dependency Services!

            handler = new Handler(this.MainLooper);

            C.DeviceID = Settings.Secure.GetString(
                    ApplicationContext.ContentResolver,
                    Settings.Secure.AndroidId);
            license_manager = C.ontainer.Resolve<ILicenseManager>();


            license_manager.BackupPreferences = 
                PreferenceManager.GetDefaultSharedPreferences(this);

            string id = "pub-1015422455885077";
            MobileAds.Initialize(ApplicationContext, id);
            System.Diagnostics.Debug.WriteLine("OnCreate()end");
        }

        void RunOnMainThread(Action action)
        {
            handler.Post(action);
        }

        async Task CheckLicense()
        {
            const int TimeDay = 86400;
            const int TimeWeek = 7 * TimeDay;
            const int WarningPeriod = 7 * TimeDay;

            bool has_license = await license_manager.CheckLicense();
            System.Diagnostics.Debug.WriteLine($"has_license = {has_license}");
            if (!has_license)
            {
                int seconds_left_in_trial = await license_manager.SecondsRemainingInTrial();
                bool is_in_trial = seconds_left_in_trial > 0;

                System.Diagnostics.Debug.WriteLine
                    ($"is_in_trial = {is_in_trial}, {seconds_left_in_trial} seconds left");

                if (!is_in_trial)
                {
                    bool bought = await license_manager.PromptToBuyLicense();
                    if(!bought)
                    {
                        RunOnMainThread(delegate
                        {
                            Toast.MakeText(CrossCurrentActivity.Current.Activity,
                                Resource.String.consider_purchase, ToastLength.Long).Show();
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"bought = {bought}");
                }
                else if(seconds_left_in_trial < WarningPeriod)
                {
                    int days_left = seconds_left_in_trial / TimeDay;
                    RunOnMainThread(delegate
                    {
                        Toast.MakeText(CrossCurrentActivity.Current.Activity,
                            $"You have {days_left} days left in your free trial. ", 
                            ToastLength.Long).Show();
                    });
                    System.Diagnostics.Debug.WriteLine($"You have {days_left} days left in your free trial. ");
                }
            }
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        volatile bool seen_activity = false;
        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Activity = activity;

            if (!seen_activity)
            {
                seen_activity = true;
                // first activity was created
                try
                {
                    CheckLicense();
                }
                catch
                {

                }
            }
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityResumed(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
            CrossCurrentActivity.Current.Activity = activity;
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}