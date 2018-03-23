using Foundation;
using UIKit;

using System.IO;

using POLift.Core.Service;
using POLift.Core.ViewModel;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;

using Unity;
using DialogService = POLift.Core.Service.DialogService;
using IDialogService = POLift.Core.Service.IDialogService;
using SQLite.Net.Platform.XamarinIOS;

namespace POLift.iOS
{
    using Controllers;
    using Service;
    using UserNotifications;

    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        static string DatabaseFileName = "polift-database.db3";
        public static string DatabaseDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        public static string DatabasePath = Path.Combine(DatabaseDirectory, DatabaseFileName);

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            System.Diagnostics.Debug.WriteLine("DatabasePath = " + DatabasePath);

            DispatcherHelper.Initialize(application);

            IPOLDatabase database = new POLDatabase(
                new SQLitePlatformIOS(),
                DatabasePath);

            SimpleIoc.Default.Register<IPOLDatabase>(() => database);

            var nav = new NavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            RootController root = (RootController)Window.RootViewController;

            //nav.Initialize((RootController)Window.RootViewController);
            nav.Initialize(root.NavController);
            nav.Configure(ViewModelLocator.MainPageKey, "MainPage");
            nav.Configure(ViewModelLocator.CreateRoutinePageKey, "CreateRoutinePage");
            nav.Configure(ViewModelLocator.SelectExercisePageKey, "SelectExercisePage");
            nav.Configure(ViewModelLocator.CreateExercisePageKey, "CreateExercisePage");
            nav.Configure(ViewModelLocator.PerformRoutinePageKey, "PerformRoutinePage");
            nav.Configure(ViewModelLocator.PerformWarmupPageKey, "PerformWarmupPage");
            nav.Configure(ViewModelLocator.ViewRoutineResultsPageKey, "ViewRoutineResultsPage");
            nav.Configure(ViewModelLocator.EditRoutineResultPageKey, "EditRoutineResultPage");
            nav.Configure(ViewModelLocator.OrmGraphPageKey, "OrmGraphRootPage");
            nav.Configure(ViewModelLocator.SelectExerciseDifficultyPageKey, "SelectExerciseDifficultyPage");
            nav.Configure(ViewModelLocator.SelectProgramToDownloadPageKey, "SelectProgramToDownloadPage");

            ViewModelLocator l = ViewModelLocator.Default;
            l.KeyValueStorage = new UserDefaultsKeyValueStorage();
            //new DatabaseKeyValueStorage(database);

            l.DialogService = new DialogService(
                new DialogBuilderFactory(), l.KeyValueStorage);
            l.Toaster = new Toaster();

            l.MainThreadInvoker = new MainThreadInvoker(application);

            string device_id = UIDevice
                .CurrentDevice.IdentifierForVendor.AsString();
            System.Console.WriteLine("device_id = " + device_id);
            l.LicenseManager = new LicenseManager(device_id, l.KeyValueStorage);
            //l.LicenseManager = new StoreKitLicenseManager(device_id, l.KeyValueStorage);
            l.LicenseManager.ProductID = "com.cml.POLift.license";

            l.SelectProgramToDownload.FileOperations = new FileOperations();

            l.Vibrator = new AppleVibrator();

            l.TimerService = new BackgroundTimer();

            l.TimerFinishedNotificationService = new NotificationService();
            
            // Request notification permissions from the user
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound, (approved, err) => {
                // Handle approval
               
            });

            l.CheckLicenseAndPrompt();

            //l.PromptUserForStartingNextRoutine(database, nav);
            //l.SideMenu.PromptUserForExternalProgramsIfFirstLaunch();
            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}