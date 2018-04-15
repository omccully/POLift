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

    public class CreateRoutineViewModel : ViewModelBase, ICreateRoutineViewModel, IValueReturner<IRoutine>
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public event Action<IRoutine> ValueChosen;
        public IToaster Toaster;

        ISelectExerciseViewModel _SelectExerciseViewModel;
        public ISelectExerciseViewModel SelectExerciseViewModel
        {
            get
            {
                return _SelectExerciseViewModel;
            }
            set
            {
                _SelectExerciseViewModel = value;
                _SelectExerciseViewModel.ValueChosen += SelectExercise_ValueChosen;
            }
        }

        public CreateRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public int RoutineToDeleteIfDifferentId
        {
            get
            {
                return RoutineToDeleteIfDifferent.ID;
            }
            set
            {
                if(value > 0)
                {
                    RoutineToDeleteIfDifferent =
                        Database.ReadByID<Routine>(value);
                }
                else
                {
                    RoutineToDeleteIfDifferent = null;
                }
            }
        }
        public IRoutine RoutineToDeleteIfDifferent;

        public int LockedExercises { get; private set; }

        int _LockedExerciseSets;
        public int LockedExerciseSets {
            get
            {
                return _LockedExerciseSets;
            }
            private set
            {
                _LockedExerciseSets = value;
            }
        }

        public Routine EditRoutine(int routine_id, int locked_sets = 0)
        {
            Routine routine = Database.ReadByID<Routine>(routine_id);
            EditRoutine(routine, locked_sets);

            return routine;
        }

        public void EditRoutine(IRoutine routine, int locked_exercises = 0)
        {
            Reset();

            if (routine != null)
            {
                RoutineToDeleteIfDifferent = routine;
                System.Diagnostics.Debug.WriteLine("routine.Name = " + routine.Name);
                RoutineNameInput = routine.Name;

                SetExerciseSets(routine.Exercises, locked_exercises);
            }
        }

        public void SetExerciseSets(IEnumerable<IExerciseSets> exercise_sets, int exercises_locked)
        {
            if(exercises_locked == 0)
            {
                ExerciseSets.SetCollection(exercise_sets);
                LockedExercises = LockedExerciseSets = 0;
            }

            SetExerciseSets(Model.ExerciseSets.Expand(exercise_sets), exercises_locked);
        }

        public void SetExerciseSets(IEnumerable<IExercise> exercises, int exercises_locked)
        {
            ExerciseSets.SetCollection(
                exercises.SplitLockedSets(exercises_locked, out _LockedExerciseSets));

            LockedExercises = exercises_locked;
        }

        public readonly ObservableCollection<IExerciseSets> ExerciseSets =
            new ObservableCollection<IExerciseSets>();

        public void Reset()
        {
            RoutineToDeleteIfDifferent = null;
            RoutineNameInput = "";
            ExerciseSets.Clear();
            LockedExercises = 0;
            LockedExerciseSets = 0;
        }

        public void SelectExercise_ValueChosen(int exercise_id)
        {
            Exercise selected_exercise = Database.ReadByID<Exercise>(exercise_id);

            SelectExercise_ValueChosen(selected_exercise);
        }

        public void SelectExercise_ValueChosen(IExercise exercise)
        {
            System.Diagnostics.Debug.WriteLine($"RoutineNameInput={RoutineNameInput} {String.IsNullOrWhiteSpace(RoutineNameInput)}");
            System.Diagnostics.Debug.WriteLine($"exercise.Category={exercise.Category} {!String.IsNullOrWhiteSpace(exercise.Category)}");

            if (String.IsNullOrWhiteSpace(RoutineNameInput)
                   && !String.IsNullOrWhiteSpace(exercise.Category))
            {
                Toaster?.DisplayMessage("Routine title set to \"" + exercise.Category +
                    "\" based on the added exercise's category");

                RoutineNameInput = exercise.Category;
            }

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
                System.Diagnostics.Debug.WriteLine($"RoutineNameInput={RoutineNameInput}");
                Set(() => RoutineNameInput, ref _RoutineNameInput, value);
            }
        }

        public List<IExerciseSets> SaveExerciseSets()
        {
            return ExerciseSets.SaveExerciseSets(Database);
        }

        public Routine CreateRoutineFromInput()
        {
            try
            {
                if (this.ExerciseSets.Count == 0)
                {
                    Toaster.DisplayError(
                        "You must have exercises in your routine. ");
                    return null;
                }

                List<IExerciseSets> normalized = SaveExerciseSets();

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
            catch(ArgumentException ae)
            {
                Toaster.DisplayError(ae.Message);
                return null;
            }
        }

        public List<IExerciseSets> IdsToExerciseSets(int[] ids)
        {
            return new List<IExerciseSets>(Database.ParseIDs<ExerciseSets>(ids));
        }

        public void AddExerciseNavigate()
        {
            SelectExerciseViewModel.Category = this.RoutineNameInput;
            navigationService.NavigateTo(
                ViewModelLocator.SelectExercisePageKey);
        }

        RelayCommand _AddExerciseCommand;
        public RelayCommand AddExerciseCommand
        {
            get
            {
                return _AddExerciseCommand ??
                    (_AddExerciseCommand =
                    new RelayCommand(AddExerciseNavigate));
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
                            Routine result = CreateRoutineFromInput();

                            if (result != null)
                            {
                                ValueChosen?.Invoke(result);
                                navigationService.GoBack();
                            }
                        }));
            }
        }

        //public const string RoutineIDToDeleteIfDifferentKey = "routine_id_to_delete_if_different";
        public const string EditRoutineIdKey = "edit_routine_id";
        public const string ExerciseSetsIdsKey = "exercise_sets_ids";
        public const string ExercisesLockedKey = "exercises_locked";

        public void RestoreState(KeyValueStorage kvs)
        {
            Reset();

            int[] ids = kvs.GetIntArray(ExerciseSetsIdsKey);

            int exercises_locked = kvs.GetInteger(ExercisesLockedKey);

            RoutineToDeleteIfDifferentId = kvs.GetInteger(EditRoutineIdKey);

            if (ids == null)
            {
                // no ES IDs, so 
                if (RoutineToDeleteIfDifferent != null)
                {
                    // edit routine
                    EditRoutine(RoutineToDeleteIfDifferent, exercises_locked);
                }
            }
            else // ids != null
            {
                SetExerciseSets(IdsToExerciseSets(ids), exercises_locked);
            }
        }

        public void SaveState(KeyValueStorage kvs)
        {
            List<IExerciseSets> saved_sets = SaveExerciseSets();

            int[] ids = saved_sets.Select(es => es.ID).ToArray();
            System.Diagnostics.Debug.WriteLine("ids during save: " + ids.ToIDString());

            kvs.SetValue(ExerciseSetsIdsKey, ids);

            InitializationState(kvs, RoutineToDeleteIfDifferent, LockedExercises);
        }

        public static void InitializationState(KeyValueStorage kvs,
            IRoutine routine = null, int exercises_locked = 0)
        {
            if(routine != null) kvs.SetValue(EditRoutineIdKey, routine.ID);

            if(exercises_locked != 0) kvs.SetValue(ExercisesLockedKey, exercises_locked);
        }
    }
}
