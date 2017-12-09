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

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
