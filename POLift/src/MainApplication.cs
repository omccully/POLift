using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

using Android.Content;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Provider;
using Android.Gms.Ads;
using Android.Preferences;
using Plugin.CurrentActivity;
using Android.Util;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

using Microsoft.Practices.Unity;


namespace POLift
{
    using Service;
    using Core.Service;
    using Core.Model;

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
        IPOLDatabase Database;

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
            //System.Diagnostics.Debug.WriteLine();

            Log.Debug("POLift", "MainApplication(IntPtr handle, JniHandleOwnership transer)");
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

            Database = C.ontainer.Resolve<IPOLDatabase>();

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

        bool first_main_activity = true;
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

            if(first_main_activity && activity.GetType() == typeof(MainActivity))
            {
                first_main_activity = false;

                PromptUserForStartingNextRoutine(activity);
               
            }
        }

        void PromptUserForStartingNextRoutine(Activity activity)
        {
            Log.Debug("POLift", "finding next routine...");
            var rrs = Database.Table<RoutineResult>().OrderByDescending(rr => rr.StartTime);
            RoutineResult latest_routine_result = rrs.ElementAtOrDefault(0);
            if (latest_routine_result == null)
            {
                Log.Debug("POLift", "no recent routine result");
                return;
            }
            if (!latest_routine_result.Completed)
            {
                int ec = latest_routine_result.ExerciseCount;
                int erc = latest_routine_result.ExerciseResults.Count();
                Log.Debug("POLift", $"latest routine result was uncompleted. ec={ec}, erc={erc}");
                //
                return;
            }

            if ((DateTime.Now - latest_routine_result.StartTime) < TimeSpan.FromHours(20))
            {
                Log.Debug("POLift", $"latest routine result was started less than 20 hours ago");
                return;
            }

            int latest_routine_id = latest_routine_result.RoutineID;
            string latest_routine_name = latest_routine_result.Routine.Name;

            int previous_routine_id = -1;
            foreach (RoutineResult rr in rrs)
            {
                Log.Debug("POLift", "checking " + rr);
                if (previous_routine_id != -1 && 
                    (rr.RoutineID == latest_routine_id || rr.Routine.Name == latest_routine_name))
                {
                    Routine next_routine = Database.ReadByID<Routine>(previous_routine_id);

                    /*Helpers.DisplayConfirmation(activity, 
                        "Based on your history, it looks like your next routine is " + 
                        $"\"{next_routine.Name}\". Would you like to do this routine now?",
                        delegate
                        {
                            Intent intent = new Intent(activity, typeof(PerformRoutineActivity));
                            intent.PutExtra("routine_id", next_routine.ID);

                            activity.StartActivity(intent);
                        },
                        delegate
                        {

                        });*/

                    Log.Debug("POLift", "next routine found");
                    AndroidHelpers.DisplayConfirmationNeverShowAgain(activity,
                        "Based on your history, it looks like your next routine is " +
                        $"\"{next_routine.Name}\". Would you like to do this routine now?",
                        "start_next_routine",
                        delegate
                        {
                            Intent intent = new Intent(activity, typeof(PerformRoutineActivity));
                            intent.PutExtra("routine_id", next_routine.ID);

                            activity.StartActivity(intent);
                        });

                    break;
                }

                previous_routine_id = rr.RoutineID;
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