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
        public const string CreateRoutinePageKey = "CreateRoutine";
        public const string SelectExercisePageKey = "SelectExercise";
        public const string CreateExercisePageKey = "CreateExercise";
        public const string PerformRoutinePageKey = "PerformRoutine";
        public const string PerformWarmupPageKey = "PerformWarmup";
        public const string ViewRoutineResultsPageKey = "ViewRoutineResults";
        public const string EditRoutineResultPageKey = "EditRoutineResult";

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

            SelectExercise.CreateExerciseViewModel = CreateExercise;
            CreateRoutine.SelectExerciseViewModel = SelectExercise;

            Main.CreateRoutineViewModel = CreateRoutine;
            Main.PerformRoutineViewModel = PerformRoutine;

            PerformRoutine.PerformWarmupViewModel = PerformWarmup;
            PerformRoutine.TimerViewModel = Timer;

            PerformWarmup.TimerViewModel = Timer;

            Timer.Timer = new PclTimer();
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
