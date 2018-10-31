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

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;

namespace POLift.Droid
{
    using Service;
    using Core.Service;
    using Core.Model;
    using Core.ViewModel;

    //You can specify additional application information in this attribute
#if DEBUG
    [Application(Debuggable=true)]
#else
    [Application(Debuggable =false)]
#endif
    public class MainApplication : Application, Application.IActivityLifecycleCallbacks
    {
        private MainViewModel MainVm
        {
            get
            {
                return ViewModelLocator.Default.Main;
            }
        }

        Handler handler;
        //ILicenseManager license_manager;
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

            string device_id = Settings.Secure.GetString(
                    ApplicationContext.ContentResolver,
                    Settings.Secure.AndroidId);

            System.Diagnostics.Debug.WriteLine("DeviceID = " + device_id);

            //license_manager.BackupPreferences = 
            //    PreferenceManager.GetDefaultSharedPreferences(this);

            Database = C.Database;

            try
            {
                MobileAds.Initialize(ApplicationContext, "ca-app-pub-1015422455885077~9023659423");
            }
            catch { }

            SimpleIoc.Default.Register<IPOLDatabase>(() => Database);

            var nav = new NavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ViewModelLocator l = ViewModelLocator.Default;
            l.KeyValueStorage = new PreferencesKeyValueStorage(
                PreferenceManager.GetDefaultSharedPreferences(this));
            //new DatabaseKeyValueStorage(database);

            //l.DialogService = new DialogService(
            //    new DialogBuilderFactory(this), l.KeyValueStorage);
            l.Toaster = new Toaster(this);

            l.MainThreadInvoker = new MainThreadInvoker(new Handler(this.MainLooper));

            l.LicenseManager = new LicenseManager(device_id, l.KeyValueStorage);

            l.SelectProgramToDownload.FileOperations = new FileOperations();

            l.Vibrator = new AndroidVibrator(this);

            l.TimerService = new PclTimer();
            // TODO: use SimpleIoc properly so that not all viewmodels are
            // instantiated when first launch

            //l.TimerFinishedNotificationService = new NotificationService();
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("VML time = " + sw.ElapsedMilliseconds);
            System.Diagnostics.Debug.WriteLine("OnCreate()end");
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(this);
        }

        bool first_main_activity = true;
        volatile bool seen_activity = false;
        //Activity last_activity_created = null;
        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("Create " + activity.GetType());

            CrossCurrentActivity.Current.Activity = activity;

            if (!seen_activity)
            {
                seen_activity = true;
                // first activity was created
                try
                {
                    ViewModelLocator.Default.CheckLicenseAndPrompt();
                }
                catch
                {

                }
            }

            if(first_main_activity && activity.GetType() == typeof(MainActivity))
            {
                first_main_activity = false;

                try
                {
                    PromptUserForStartingNextRoutine(activity);
                }
                catch { }
            }

            /*ViewModelLocator.Default.DialogService = new DialogService(
                new DialogBuilderFactory(activity),
                ViewModelLocator.Default.KeyValueStorage);

            ViewModelLocator.Default.Vibrator = new AndroidVibrator(activity);
            */


            //if(last_activity_created.GetType == typeof(MainActivity))
            //{
            //
            //}

            //last_activity_created = activity;
        }

        void PromptUserForStartingNextRoutine(Activity activity)
        {
            MainVm.PromptUserForStartingNextRoutine(delegate (IRoutine next_routine)
            {
                Intent intent = new Intent(activity, typeof(PerformRoutineActivity));
                intent.PutExtra("routine_id", next_routine.ID);

                activity.StartActivity(intent);
            });
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