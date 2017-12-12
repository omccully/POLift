using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

namespace POLift.Core.ViewModel
{
    using Service;
    using Model;

    public class CreateRoutineViewModel : ViewModelBase, IValueReturner<IRoutine>
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public event Action<IRoutine> ValueChosen;
        public IDialogMessageService DialogService;

        public CreateRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;

            ViewModelLocator.Default.SelectExercise.ValueChosen += SelectExercise_ValueChosen;
        }

        IRoutine RoutineToDeleteIfDifferent;
        public int LockedSets { get; set; }

        int _LockedExerciseSets;
        public int LockedExerciseSets {
            get
            {
                return _LockedExerciseSets;
            }
            set
            {
                _LockedExerciseSets = value;
            }
        }

        public void EditRoutine(IRoutine routine, int locked_sets = 0)
        {
            Reset();

            if (routine != null)
            {
                RoutineToDeleteIfDifferent = routine;
                RoutineNameInput = routine.Name;

                ExerciseSets.Clear();

                LockedSets = locked_sets;
                IEnumerable<IExerciseSets> exercise_sets = 
                    Model.ExerciseSets.Expand(routine.ExerciseSets)
                    .SplitLockedSets(locked_sets, out _LockedExerciseSets);

                foreach (IExerciseSets es in exercise_sets)
                {
                    ExerciseSets.Add(es);
                }
            }
        }
           
        public readonly ObservableCollection<IExerciseSets> ExerciseSets =
            new ObservableCollection<IExerciseSets>();

        void Reset()
        {
            RoutineToDeleteIfDifferent = null;
            RoutineNameInput = "";
            ExerciseSets.Clear();
            LockedSets = 0;
            LockedExerciseSets = 0;
        }

        private void SelectExercise_ValueChosen(IExercise exercise)
        {
            // add exercise to ExerciseSets list

            ExerciseSets es = new ExerciseSets(exercise);
            es.Database = Database;
            this.ExerciseSets.Add(es);
            // the add triggers CollectionChanged, so the view should update 
            // when that event occurs.
        }

        string _RoutineNameInput;
        public string RoutineNameInput
        {
            get
            {
                return _RoutineNameInput;
            }
            set
            {
                Set(() => RoutineNameInput, ref _RoutineNameInput, value);
            }
        }

        Routine CreateRoutineFromInput()
        {
            if (this.ExerciseSets.Count == 0)
            {
                DialogService?.DisplayTemporaryError(
                    "You must have exercises in your routine. ");
                return null;
            }

            List<IExerciseSets> normalized =
                this.ExerciseSets.SaveExerciseSets(Database);

            Routine routine = new Routine(RoutineNameInput,
                   normalized);
            routine.Database = Database;

            Database.InsertOrUndeleteAndUpdate(routine);

            // if this routine is being edited, then delete the old one
            if (RoutineToDeleteIfDifferent != null &&
                !routine.Equals(RoutineToDeleteIfDifferent))
            {
                Database.HideDeletable<Routine>((Routine)RoutineToDeleteIfDifferent);
            }

            // set the category for all of the exercises in this routine
            foreach (IExerciseSets ex_sets in normalized)
            {
                Exercise ex = ex_sets.Exercise;
                ex.Category = routine.Name;
                Database.Update(ex);
            }

            return routine;
        }

        RelayCommand _AddExerciseCommand;
        public RelayCommand AddExerciseCommand
        {
            get
            {
                return _AddExerciseCommand ??
                    (_AddExerciseCommand =
                    new RelayCommand(
                        () => navigationService.NavigateTo(
                                ViewModelLocator.SelectExercisePageKey)));
            }
        }

        RelayCommand _CreateRoutineCommand;
        public RelayCommand CreateRoutineCommand
        {
            get
            {
                return _CreateRoutineCommand ??
                    (_CreateRoutineCommand =
                    new RelayCommand(
                        () => {
                            try
                            {
                                Routine result = CreateRoutineFromInput();

                                if (result != null)
                                {
                                    ValueChosen?.Invoke(result);
                                    navigationService.GoBack();
                                }
                            }
                            catch (ArgumentException ae)
                            {
                                DialogService?.DisplayTemporaryError(ae.Message);
                            }
                        }));
            }
        }


    }
}
