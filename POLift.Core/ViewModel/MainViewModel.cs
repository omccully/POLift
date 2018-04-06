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

        ICreateRoutineViewModel _CreateRoutineViewModel;
        public ICreateRoutineViewModel CreateRoutineViewModel
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
                //if (_RoutinesList == null)
                RefreshRoutinesList();

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

        public void PerformRoutineOnTheFlyNavigate()
        {
            // navigationService.NavigateTo(ViewModelLocator.PerformRoutinePageKey);
            ViewModelLocator.Default.PerformRoutine.PerformRoutineOnTheFly();
        }

        private RelayCommand _PerformRoutineOnTheFlyNavigateCommand;
        public RelayCommand PerformRoutineOnTheFlyNavigateCommand
        {
            get
            {
                return _PerformRoutineOnTheFlyNavigateCommand
                    ?? (_PerformRoutineOnTheFlyNavigateCommand =
                        new RelayCommand(PerformRoutineOnTheFlyNavigate));
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
                CreateRoutineViewModel.Reset();
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


        public void PromptUserForStartingNextRoutine(Action<IRoutine> navigate_to_routine = null)
        {
            System.Diagnostics.Debug.WriteLine("finding next routine...");
            var rrs = database.Table<RoutineResult>().OrderByDescending(rr => rr.StartTime);
            RoutineResult latest_routine_result = rrs.ElementAtOrDefault(0);
            if (latest_routine_result == null)
            {
                System.Diagnostics.Debug.WriteLine("no recent routine result");
                return;
            }
            if (!latest_routine_result.Completed)
            {
                int ec = latest_routine_result.ExerciseCount;
                int erc = latest_routine_result.ExerciseResults.Count();
                System.Diagnostics.Debug.WriteLine($"latest routine result was uncompleted. ec={ec}, erc={erc}");
                //
                return;
            }

            if ((DateTime.Now - latest_routine_result.StartTime) < TimeSpan.FromHours(20))
            {
                System.Diagnostics.Debug.WriteLine($"latest routine result was started less than 20 hours ago");
                return;
            }

            int latest_routine_id = latest_routine_result.RoutineID;
            string latest_routine_name = latest_routine_result.Routine.Name;

            int previous_routine_id = -1;
            foreach (RoutineResult rr in rrs)
            {
                System.Diagnostics.Debug.WriteLine("checking " + rr);
                if (previous_routine_id != -1 &&
                    (rr.RoutineID == latest_routine_id || rr.Routine.Name == latest_routine_name))
                {
                    Routine next_routine = database.ReadByID<Routine>(previous_routine_id);

                    System.Diagnostics.Debug.WriteLine("next routine found");

                    DialogService.DisplayConfirmationNeverShowAgain(
                        "Based on your history, it looks like your next routine is " +
                        $"\"{next_routine.Name}\". Would you like to do this routine now?",
                        "start_next_routine",
                        delegate
                        {
                            if(navigate_to_routine == null)
                            {
                                ViewModelLocator.Default.PerformRoutine.Routine = next_routine;
                                navigationService.NavigateTo(
                                    ViewModelLocator.PerformRoutinePageKey);
                            }
                            else
                            {
                                navigate_to_routine(next_routine);
                            }
                        });

                    break;
                }

                previous_routine_id = rr.RoutineID;
            }
        }
    }
}
