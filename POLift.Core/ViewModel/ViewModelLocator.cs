using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight; using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace POLift.Core.ViewModel
{
    public class ViewModelLocator
    {
        public const string CreateRoutinePageKey = "CreateRoutine";
        public const string SelectExercisePageKey = "SelectExercise";
        public const string CreateExercisePageKey = "CreateExercise";
        public const string PerformRoutinePageKey = "PerformRoutine";


        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PerformRoutineViewModel>();
            SimpleIoc.Default.Register<SelectExerciseViewModel>();
            SimpleIoc.Default.Register<CreateExerciseViewModel>();
            SimpleIoc.Default.Register<CreateRoutineViewModel>();
        }          public MainViewModel Main
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
