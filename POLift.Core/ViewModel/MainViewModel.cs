using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

namespace POLift.Core.ViewModel
{
    using Model;
    using Service;

    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase database;

        public MainViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.database = database;
        }

        private IEnumerable<IRoutineWithLatestResult> _RoutinesList;
        public IEnumerable<IRoutineWithLatestResult> RoutinesList
        {
            get
            {
                return _RoutinesList
                    ?? (_RoutinesList = Helpers.MainPageRoutinesList(database));
            }
        }

        private RelayCommand navigateCommand;
        public RelayCommand CreateRoutineNavigateCommand
        {
            get
            {
                return navigateCommand
                    ?? (navigateCommand =
                        new RelayCommand(()
                        => navigationService.NavigateTo(
                            ViewModelLocator.CreateRoutinePageKey)));
            }
        }

        public RelayCommand SelectRoutineNavigateCommand(IRoutineWithLatestResult selection)
        {
            return SelectRoutineNavigateCommand(selection.Routine);
        }

        public RelayCommand SelectRoutineNavigateCommand(IRoutine selection)
        {
            return new RelayCommand(() => {
                ViewModelLocator.Default.PerformRoutine.Routine = selection;

                navigationService.NavigateTo(
                        ViewModelLocator.PerformRoutinePageKey, selection);
            });
        }
    }
}
