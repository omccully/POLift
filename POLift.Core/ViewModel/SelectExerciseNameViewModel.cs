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
    public class SelectExerciseNameViewModel : SelectExerciseGroupViewModel
    {
        public SelectExerciseNameViewModel(INavigationService navigationService,
            IPOLDatabase database) : base(navigationService, database)
        {

        }

        public override List<ExerciseGroupCategory> ExerciseCategories
        {
            get
            {
                ExerciseDifficulty.Regenerate(Database);
                ExerciseDifficulty.RefreshAllUsages(Database);

                return ExerciseName.InEnCategories(Database);
            }
        }
    }
}
