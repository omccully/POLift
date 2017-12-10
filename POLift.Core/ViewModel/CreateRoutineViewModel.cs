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

        public CreateRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;

            ViewModelLocator.Default.SelectExercise.ValueChosen += SelectExercise_ValueChosen;
        }

        public readonly ObservableCollection<IExerciseSets> ExerciseSets =
            new ObservableCollection<IExerciseSets>();

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

        void CreateRoutineFromInput()
        {
            if (this.ExerciseSets.Count == 0)
            {
                // "You must have exercises in your routine. "
                return;
            }

            List<IExerciseSets> normalized =
                this.ExerciseSets.SaveExerciseSets(Database);

            Routine routine = new Routine(RoutineNameInput,
                   normalized);
            routine.Database = Database;

            Database.InsertOrUndeleteAndUpdate(routine);

            // set the category for all of the exercises in this routine
            foreach (IExerciseSets ex_sets in normalized)
            {
                Exercise ex = ex_sets.Exercise;
                ex.Category = routine.Name;
                Database.Update(ex);
            }

            ValueChosen?.Invoke(routine);
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
                                ViewModelLocator.CreateExercisePageKey)));
            }
        }
    }
}
