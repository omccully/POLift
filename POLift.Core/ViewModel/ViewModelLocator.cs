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
