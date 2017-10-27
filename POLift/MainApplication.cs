using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Provider;
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

            
        }

        async Task CheckLicense()
        {
            string device_id = Settings.Secure.GetString(
                   ApplicationContext.ContentResolver,
                   Settings.Secure.AndroidId);

            bool has_license = await LicenseManager.CheckLicense();
            System.Diagnostics.Debug.WriteLine($"has_license = {has_license}");
            if (!has_license)
            {
                bool is_in_trial = await LicenseManager.IsInTrialPeriod(device_id);
                System.Diagnostics.Debug.WriteLine($"is_in_trial = {is_in_trial}");
                if (!is_in_trial)
                {
                    bool bought = await LicenseManager.PromptToBuyLicense();

                    System.Diagnostics.Debug.WriteLine($"bought = {bought}");
                }
            }
        }


        /*void FinishWebRequest(IAsyncResult result)
        {
            try
            {
                WebResponse web_response = WebRequest.EndGetResponse(result);

                string str_response;
                using (StreamReader reponse_reader = new StreamReader(web_response.GetResponseStream()))
                {
                    str_response = reponse_reader.ReadToEnd();
                }
                
                new Handler(this.MainLooper).Post(delegate
                {
                    Helpers.DisplayError(CrossCurrentActivity.Current.Activity, str_response);
                });

            }
            catch
            {
            }
        }*/

        

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