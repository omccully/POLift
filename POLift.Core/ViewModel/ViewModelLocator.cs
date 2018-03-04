using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;

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
            PerformRoutine.CreateExerciseViewModel = CreateExercise;
            
            PerformWarmup.TimerViewModel = Timer;

            ViewRoutineResults.EditRoutineResultViewModel = EditRoutineResult;

            OrmGraph.SelectExerciseDifficultyViewModel = SelectExerciseDifficulty;

            TimerService = new PclTimer();
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

                Main.DialogService = value;
                PerformRoutine.DialogService = value;
                PerformWarmup.DialogService = value;
                SelectExercise.DialogService = value;
                ViewRoutineResults.DialogService = value;
                SelectProgramToDownload.DialogService = value;
                SideMenu.DialogService = value;
            }
        }

        Timer _TimerService;
        public Timer TimerService
        {
            get
            {
                return _TimerService;
            }
            set
            {
                _TimerService = value;

                Timer.Timer = value;
            }
        }

        INotificationService _TimerFinishedNotificationService;
        public INotificationService TimerFinishedNotificationService
        {
            get
            {
                return _TimerFinishedNotificationService;
            }
            set
            {
                _TimerFinishedNotificationService = value;
                Timer.TimerFinishedNotificationService = value;
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

        IVibrator _Vibrator;
        public IVibrator Vibrator
        {
            get
            {
                return _Vibrator;
            }
            set
            {
                _Vibrator = value;

                Timer.Vibrator = value;
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
                SideMenu.MainThreadInvoker = value;
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
                return SimpleIoc.Default.GetInstance<MainViewModel>();
            }
        }

        public PerformRoutineViewModel PerformRoutine
        {
            get
            {
                return SimpleIoc.Default.GetInstance<PerformRoutineViewModel>();
            }
        }

        public PerformWarmupViewModel PerformWarmup
        {
            get
            {
                return SimpleIoc.Default.GetInstance<PerformWarmupViewModel>();
            }
        }

        public SelectExerciseViewModel SelectExercise
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SelectExerciseViewModel>();
            }
        }

        public CreateExerciseViewModel CreateExercise
        {
            get
            {
                return SimpleIoc.Default.GetInstance<CreateExerciseViewModel>();
            }
        }

        public CreateRoutineViewModel CreateRoutine
        {
            get
            {
                return SimpleIoc.Default.GetInstance<CreateRoutineViewModel>();
            }
        }

        public TimerViewModel Timer
        {
            get
            {
                return SimpleIoc.Default.GetInstance<TimerViewModel>();
            }
        }

        public ViewRoutineResultsViewModel ViewRoutineResults
        {
            get
            {
                return SimpleIoc.Default.GetInstance<ViewRoutineResultsViewModel>();
            }
        }

        public EditRoutineResultViewModel EditRoutineResult
        {
            get
            {
                return SimpleIoc.Default.GetInstance<EditRoutineResultViewModel>();
            }
        }

        public OrmGraphViewModel OrmGraph
        {
            get
            {
                return SimpleIoc.Default.GetInstance<OrmGraphViewModel>();
            }
        }

        public SelectExerciseDifficultyViewModel SelectExerciseDifficulty
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SelectExerciseDifficultyViewModel>();
            }
        }

        public SideMenuViewModel SideMenu
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SideMenuViewModel>();
            }
        }

        public SelectProgramToDownloadViewModel SelectProgramToDownload
        {
            get
            {
                return SimpleIoc.Default.GetInstance<SelectProgramToDownloadViewModel>();
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
