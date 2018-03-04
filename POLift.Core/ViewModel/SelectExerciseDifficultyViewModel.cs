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
    public class SelectExerciseDifficultyViewModel : ViewModelBase, IValueReturner<IExerciseDifficulty>
    {
        public event Action<IExerciseDifficulty> ValueChosen;

        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public SelectExerciseDifficultyViewModel(INavigationService navigationService,
            IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public List<ExerciseDifficultyCategory> ExerciseDifficultyCategories
        {
            get
            {
                ExerciseDifficulty.Regenerate(Database);
                ExerciseDifficulty.RefreshAllUsages(Database);

                return ExerciseDifficulty.InEdCategories(Database);
            }
        }

        public void ReturnExerciseDifficulty(IExerciseDifficulty ed)
        {
            ValueChosen?.Invoke(ed);
            navigationService.GoBack();
        }
    }
}
