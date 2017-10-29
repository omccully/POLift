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
using Plugin.CurrentActivity;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;


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
        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(this);
            //A great place to initialize Xamarin.Insights and Dependency Services!

            string id = "pub-1015422455885077";
            MobileAds.Initialize(ApplicationContext, id);
        }

        async Task CheckLicense()
        {
            const int TimeDay = 86400;
            const int TimeWeek = 7 * TimeDay;
            const int WarningPeriod = 7 * TimeDay;

            string device_id = Settings.Secure.GetString(
                   ApplicationContext.ContentResolver,
                   Settings.Secure.AndroidId);

            bool has_license = await LicenseManager.CheckLicense();
            System.Diagnostics.Debug.WriteLine($"has_license = {has_license}");
            if (!has_license)
            {
                int seconds_left_in_trial = await LicenseManager.SecondsRemainingInTrial(device_id);
                bool is_in_trial = seconds_left_in_trial > 0;

                System.Diagnostics.Debug.WriteLine
                    ($"is_in_trial = {is_in_trial}, {seconds_left_in_trial} seconds left");

                if (!is_in_trial)
                {
                    bool bought = await LicenseManager.PromptToBuyLicense();
                    if(!bought)
                    {
                        /*typeof(NoLicenseActivity)
                        StartActivity(CrossCurrentActivity.Current.Activity,
                            );*/
                    }
                    System.Diagnostics.Debug.WriteLine($"bought = {bought}");
                }
                else if(seconds_left_in_trial < WarningPeriod)
                {
                    int days_left = seconds_left_in_trial / TimeDay;
                    new Handler(this.MainLooper).Post(delegate
                    {
                        Toast.MakeText(CrossCurrentActivity.Current.Activity,
                            $"You have {days_left} days left in your free trial. ", 
                            ToastLength.Long).Show();
                    });
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
                CheckLicense();
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