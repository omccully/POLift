using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using POLift.Core.Model;
using GalaSoft.MvvmLight.Views;
using POLift.Core.Service;
using GalaSoft.MvvmLight;

namespace POLift.Core.ViewModel
{
    public abstract class SelectExerciseGroupViewModel : 
        ViewModelBase, IValueReturner<IExerciseGroup>
    {
        public event Action<IExerciseGroup> ValueChosen;

        protected readonly INavigationService navigationService;
        protected readonly IPOLDatabase Database;

        public SelectExerciseGroupViewModel(INavigationService navigationService,
            IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public abstract List<ExerciseGroupCategory> ExerciseCategories
        {
            get;
        }

        public void ReturnExerciseGroup(IExerciseGroup exercise_group)
        {
            ValueChosen?.Invoke(exercise_group);
            navigationService.GoBack();
        }
    }
}
