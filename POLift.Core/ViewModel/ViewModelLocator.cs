using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;


namespace POLift.Core.ViewModel
{
    using GalaSoft.MvvmLight.Views;
    using Model;
    using Service;

    public class ViewModelLocator
    {
        public const string MainPageKey = "Main";
        public const string CreateRoutinePageKey = "CreateRoutine";
        public const string SelectExercisePageKey = "SelectExercise";
        public const string CreateExercisePageKey = "CreateExercise";
        public const string PerformRoutinePageKey = "PerformRoutine";
        public const string PerformWarmupPageKey = "PerformWarmup";
        public const string ViewRoutineResultsPageKey = "ViewRoutineResults";
        public const string EditRoutineResultPageKey = "EditRoutineResult";
        public const string OrmGraphPageKey = "OrmGraph";
        public const string SelectExerciseDifficultyPageKey = "SelectExerciseDifficulty";
        public const string SelectProgramToDownloadPageKey = "SelectProgramToDownload";

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                var nav = new DesignNavigationService();
                SimpleIoc.Default.Register<INavigationService>(() => nav);
               
            }
            else
            {
                // Create run time view services and models
                
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PerformRoutineViewModel>();
            SimpleIoc.Default.Register<SelectExerciseViewModel>();
            SimpleIoc.Default.Register<CreateExerciseViewModel>();
            SimpleIoc.Default.Register<CreateRoutineViewModel>();
            SimpleIoc.Default.Register<PerformWarmupViewModel>();
            SimpleIoc.Default.Register<ViewRoutineResultsViewModel>();
            SimpleIoc.Default.Register<TimerViewModel>();
            SimpleIoc.Default.Register<EditRoutineResultViewModel>();
            SimpleIoc.Default.Register<OrmGraphViewModel>();
            SimpleIoc.Default.Register<SelectExerciseDifficultyViewModel>();
            SimpleIoc.Default.Register<SideMenuViewModel>();
            SimpleIoc.Default.Register<SelectProgramToDownloadViewModel>();

            SelectExercise.CreateExerciseViewModel = CreateExercise;
            CreateRoutine.SelectExerciseViewModel = SelectExercise;

            Main.CreateRoutineViewModel = CreateRoutine;
            Main.PerformRoutineViewModel = PerformRoutine;

            PerformRoutine.PerformWarmupViewModel = PerformWarmup;
            PerformRoutine.TimerViewModel = Timer;
            PerformRoutine.EditRoutineResultViewModel = EditRoutineResult;
            PerformRoutine.CreateRoutineViewModel = CreateRoutine;
            
            PerformWarmup.TimerViewModel = Timer;

            ViewRoutineResults.EditRoutineResultViewModel = EditRoutineResult;

            OrmGraph.SelectExerciseDifficultyViewModel = SelectExerciseDifficulty;

            Timer.Timer = new PclTimer();
        }

        DialogService _DialogService;
        public DialogService DialogService
        {
            get
            {
                return _DialogService;
            }
            set
            {
                _DialogService = value;

                PerformRoutine.DialogService = value;
                PerformWarmup.DialogService = value;
                SelectExercise.DialogService = value;
                ViewRoutineResults.DialogService = value;
            }
        }

        IToaster _Toaster;
        public IToaster Toaster
        {
            get
            {
                return _Toaster;
            }
            set
            {
                _Toaster = value;

                Main.Toaster = value;
                CreateExercise.Toaster = value;
                CreateRoutine.Toaster = value;
            }
        }

        KeyValueStorage _KeyValueStorage;
        public KeyValueStorage KeyValueStorage
        {
            get
            {
                return _KeyValueStorage;
            }
            set
            {
                _KeyValueStorage = value;

                CreateExercise.KeyValueStorage = value;
                SideMenu.KeyValueStorage = value;

                if(LicenseManager != null)
                {
                    LicenseManager.KeyValueStorage = value;
                }
            }
        }

        IMainThreadInvoker _MainThreadInvoker;
        public IMainThreadInvoker MainThreadInvoker
        {
            get
            {
                return _MainThreadInvoker;
            }
            set
            {
                _MainThreadInvoker = value;

                Timer.MainThreadInvoker = value;
            }
        }

        ILicenseManager _LicenseManager;
        public ILicenseManager LicenseManager
        {
            get
            {
                return _LicenseManager;
            }
            set
            {
                _LicenseManager = value;

                if(_LicenseManager != null &&
                    _LicenseManager.KeyValueStorage == null &&
                    KeyValueStorage != null)
                {
                    _LicenseManager.KeyValueStorage = KeyValueStorage;
                }

                SideMenu.LicenseManager = value;
            }
        }

        public void PromptUserForStartingNextRoutine(IPOLDatabase Database, INavigationService nav)
        {
            System.Diagnostics.Debug.WriteLine("finding next routine...");
            var rrs = Database.Table<RoutineResult>().OrderByDescending(rr => rr.StartTime);
            RoutineResult latest_routine_result = rrs.ElementAtOrDefault(0);
            if (latest_routine_result == null)
            {
                System.Diagnostics.Debug.WriteLine("no recent routine result");
                return;
            }
            if (!latest_routine_result.Completed)
            {
                int ec = latest_routine_result.ExerciseCount;
                int erc = latest_routine_result.ExerciseResults.Count();
                System.Diagnostics.Debug.WriteLine($"latest routine result was uncompleted. ec={ec}, erc={erc}");
                //
                return;
            }

            if ((DateTime.Now - latest_routine_result.StartTime) < TimeSpan.FromHours(20))
            {
                System.Diagnostics.Debug.WriteLine($"latest routine result was started less than 20 hours ago");
                return;
            }

            int latest_routine_id = latest_routine_result.RoutineID;
            string latest_routine_name = latest_routine_result.Routine.Name;

            int previous_routine_id = -1;
            foreach (RoutineResult rr in rrs)
            {
                System.Diagnostics.Debug.WriteLine("checking " + rr);
                if (previous_routine_id != -1 &&
                    (rr.RoutineID == latest_routine_id || rr.Routine.Name == latest_routine_name))
                {
                    Routine next_routine = Database.ReadByID<Routine>(previous_routine_id);

                    System.Diagnostics.Debug.WriteLine("next routine found");

                    DialogService.DisplayConfirmationNeverShowAgain(
                        "Based on your history, it looks like your next routine is " +
                        $"\"{next_routine.Name}\". Would you like to do this routine now?",
                        "start_next_routine",
                        delegate
                        {
                            PerformRoutine.Routine = next_routine;
                            nav.NavigateTo(PerformRoutinePageKey);
                        });

                    break;
                }

                previous_routine_id = rr.RoutineID;
            }
        }

        public async Task CheckLicenseAndPrompt()
        {
            try
            {
                const int TimeDay = 86400;
                const int TimeWeek = 7 * TimeDay;
                const int WarningPeriod = 7 * TimeDay;

                bool has_license = await LicenseManager.CheckLicense();
                System.Diagnostics.Debug.WriteLine($"has_license = {has_license}");
                if (!has_license)
                {
                    int seconds_left_in_trial = await LicenseManager.SecondsRemainingInTrial();
                    bool is_in_trial = seconds_left_in_trial > 0;

                    System.Diagnostics.Debug.WriteLine
                        ($"is_in_trial = {is_in_trial}, {seconds_left_in_trial} seconds left");

                    if (!is_in_trial)
                    {
                        bool bought = await LicenseManager.PromptToBuyLicense();
                        if (!bought)
                        {
                            MainThreadInvoker.Invoke(delegate
                            {
                                Toaster.DisplayMessage("Please consider purchasing a lifetime license to remove ads.");

                            });
                        }
                        System.Diagnostics.Debug.WriteLine($"bought = {bought}");
                    }
                    else if (seconds_left_in_trial < WarningPeriod)
                    {
                        int days_left = seconds_left_in_trial / TimeDay;

                        MainThreadInvoker.Invoke(delegate
                        {
                            Toaster.DisplayMessage($"You have {days_left} days left in your free trial. ");
                        });

                        System.Diagnostics.Debug.WriteLine($"You have {days_left} days left in your free trial. ");
                    }
                }
            }
            catch { }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public PerformRoutineViewModel PerformRoutine
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PerformRoutineViewModel>();
            }
        }

        public PerformWarmupViewModel PerformWarmup
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PerformWarmupViewModel>();
            }
        }

        public SelectExerciseViewModel SelectExercise
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SelectExerciseViewModel>();
            }
        }

        public CreateExerciseViewModel CreateExercise
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CreateExerciseViewModel>();
            }
        }

        public CreateRoutineViewModel CreateRoutine
        {
            get
            {
                return ServiceLocator.Current.GetInstance<CreateRoutineViewModel>();
            }
        }

        public TimerViewModel Timer
        {
            get
            {
                return ServiceLocator.Current.GetInstance<TimerViewModel>();
            }
        }

        public ViewRoutineResultsViewModel ViewRoutineResults
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ViewRoutineResultsViewModel>();
            }
        }

        public EditRoutineResultViewModel EditRoutineResult
        {
            get
            {
                return ServiceLocator.Current.GetInstance<EditRoutineResultViewModel>();
            }
        }

        public OrmGraphViewModel OrmGraph
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OrmGraphViewModel>();
            }
        }

        public SelectExerciseDifficultyViewModel SelectExerciseDifficulty
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SelectExerciseDifficultyViewModel>();
            }
        }

        public SideMenuViewModel SideMenu
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SideMenuViewModel>();
            }
        }

        public SelectProgramToDownloadViewModel SelectProgramToDownload
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SelectProgramToDownloadViewModel>();
            }
        }

        private static ViewModelLocator _default_locator;
        public static ViewModelLocator Default
        {
            get
            {
                return _default_locator ?? (_default_locator = new ViewModelLocator());
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
