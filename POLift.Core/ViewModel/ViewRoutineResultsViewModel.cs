using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

using Xamarin.Social.Services;
using Xamarin.Social;

namespace POLift.Core.ViewModel
{
    using Service;
    using Model;

    public class ViewRoutineResultsViewModel
    {
        public DialogService DialogService;

        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public IEditRoutineResultViewModel EditRoutineResultViewModel;

        public ViewRoutineResultsViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public IEnumerable<IRoutineResult> RoutineResults
        {
            get
            {
                return Database.Table<RoutineResult>()
                    .Where(rr => !rr.Deleted)
                    .OrderByDescending(rr => rr.EndTime);
            }
        }

        public void DeleteRoutineResult(IRoutineResult rr, Action action_if_yes=null)
        {
            DialogService?.DisplayConfirmation(
                "Are you sure you want to delete this workout session? " +
                $"{rr.Routine.Name} at {rr.StartTime}", delegate
                {
                    action_if_yes?.Invoke();

                    Database.HideDeletable((RoutineResult)rr);

                    // delete all child ExerciseResults
                    foreach (IExerciseResult ex_r in rr.ExerciseResults)
                    {
                        Database.HideDeletable((ExerciseResult)ex_r);
                    }
                });
        }

        public void NavigateEditRoutineResult(IRoutineResult rr)
        {
            navigationService.NavigateTo(
                ViewModelLocator.EditRoutineResultPageKey);
            EditRoutineResultViewModel.RoutineResult = rr;
        }
    }
}
