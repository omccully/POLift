﻿using Foundation;
using UIKit;

using POLift.Core.Service;
using POLift.Core.ViewModel;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;

using Unity;
using DialogService = POLift.Core.Service.DialogService;
using IDialogService = POLift.Core.Service.IDialogService;

namespace POLift.iOS
{
    using Service;

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

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            DispatcherHelper.Initialize(application);

            IPOLDatabase database = C.ontainer.Resolve<IPOLDatabase>();
            SimpleIoc.Default.Register<IPOLDatabase>(() =>
                database);

            var nav = new NavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            nav.Initialize((UINavigationController)Window.RootViewController);
            nav.Configure(ViewModelLocator.CreateRoutinePageKey, "CreateRoutinePage");
            nav.Configure(ViewModelLocator.SelectExercisePageKey, "SelectExercisePage");
            nav.Configure(ViewModelLocator.CreateExercisePageKey, "CreateExercisePage");
            nav.Configure(ViewModelLocator.PerformRoutinePageKey, "PerformRoutinePage");
            nav.Configure(ViewModelLocator.PerformWarmupPageKey, "PerformWarmupPage");


            UserDefaultsKeyValueStorage storage =
                new UserDefaultsKeyValueStorage();
                //new DatabaseKeyValueStorage(database);
            Toaster Toaster = new Toaster();
            DialogService DialogService = new DialogService(
                new DialogBuilderFactory(), storage);

            ViewModelLocator.Default.Main.Toaster = Toaster;
            ViewModelLocator.Default.CreateExercise.KeyValueStorage = storage;
            ViewModelLocator.Default.CreateExercise.Toaster = Toaster;
            ViewModelLocator.Default.CreateRoutine.Toaster = Toaster;
            ViewModelLocator.Default.PerformRoutine.DialogService = DialogService;
            ViewModelLocator.Default.PerformWarmup.DialogService = DialogService;
            ViewModelLocator.Default.SelectExercise.DialogService = DialogService;
            ViewModelLocator.Default.Timer.MainThreadInvoker = new MainThreadInvoker(application);

            //nav.GetAndRemoveParameter()
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