using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public event EventHandler RoutinesListChanged;

        public MainViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.database = database;

            ViewModelLocator.Default.CreateRoutine.ValueChosen += CreateRoutine_ValueChosen;
        }

        private void CreateRoutine_ValueChosen(IRoutine obj)
        {
            //RoutinesList.Insert(0, new RoutineWithLatestResult(obj, null));

            RefreshRoutinesList();
        }

        private IEnumerable<IRoutineWithLatestResult> _RoutinesList;
        public IEnumerable<IRoutineWithLatestResult> RoutinesList
        {
            get
            {
                if (_RoutinesList == null) RefreshRoutinesList();

                return _RoutinesList;
            }
        }

        void RefreshRoutinesList()
        {
            _RoutinesList = Helpers.MainPageRoutinesList(database);
            RoutinesListChanged?.Invoke(this, new EventArgs());
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

        public RelayCommand EditRoutineNavigateCommand(IRoutine selection)
        {
            return new RelayCommand(() => {
                ViewModelLocator.Default.CreateRoutine.EditRoutine(selection);

                navigationService.NavigateTo(
                        ViewModelLocator.CreateRoutinePageKey, selection);
            });
        }
    }
}
