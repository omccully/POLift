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

        public IToaster Toaster;
        public event EventHandler RoutinesListChanged;

        public DialogService DialogService;

        IValueReturner<IRoutine> _CreateRoutineViewModel;
        public IValueReturner<IRoutine> CreateRoutineViewModel
        {
            get
            {
                return _CreateRoutineViewModel;
            }
            set
            {
                _CreateRoutineViewModel = value;
                _CreateRoutineViewModel.ValueChosen += CreateRoutine_ValueChosen;
            }
        }

        IValueReturner<IRoutineResult> _PerformRoutineViewModel;
        public IValueReturner<IRoutineResult> PerformRoutineViewModel
        {
            get
            {
                return _PerformRoutineViewModel;
            }
            set
            {
                _PerformRoutineViewModel = value;
                _PerformRoutineViewModel.ValueChosen += PerformRoutine_ValueChosen;
            }
        }

        public MainViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.database = database;
        }

        private void PerformRoutine_ValueChosen(IRoutineResult obj)
        {
            Toaster?.DisplayMessage("Routine completed");
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

        public void RefreshRoutinesList(bool raise_event = false)
        {
            _RoutinesList = Helpers.MainPageRoutinesList(database);
            if(raise_event)
            {
                RoutinesListChanged?.Invoke(this, new EventArgs());
            }
        }

        private RelayCommand navigateCommand;
        public RelayCommand CreateRoutineNavigateCommand
        {
            get
            {
                return navigateCommand
                    ?? (navigateCommand =
                        new RelayCommand(CreateRoutineNavigate));
            }
        }

        void CreateRoutineNavigate()
        {
            try
            {
                navigationService.NavigateTo(
                    ViewModelLocator.CreateRoutinePageKey);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
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
                EditRoutineNavigation(selection);
            });
        }

        public void EditRoutineNavigation(IRoutine selection)
        {
            ViewModelLocator.Default.CreateRoutine.EditRoutine(selection);

            navigationService.NavigateTo(
                    ViewModelLocator.CreateRoutinePageKey, selection);
        }

        public RelayCommand DeleteRoutineCommand(IRoutine selection, Action action_if_yes = null)
        {
            return new RelayCommand(() => {
                DeleteRoutine(selection, action_if_yes);
            });
        }

        public void DeleteRoutine(IRoutineWithLatestResult selection, Action action_if_yes = null)
        {
            DeleteRoutine(selection.Routine, action_if_yes);
        }

        public void DeleteRoutine(IRoutine selection, Action action_if_yes = null)
        {
            DialogService.DisplayConfirmation(
                    "Are you sure you want to delete the routine \"" +
                    selection.Name + "\"?",
                    delegate
                    {
                        database.HideDeletable((Routine)selection);

                        // additional gui actions
                        action_if_yes?.Invoke();
                    });
        }

    }
}
