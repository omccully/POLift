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

    public class SelectExerciseViewModel : ViewModelBase, IValueReturner<IExercise>
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public DialogService DialogService;

        public event Action<IExercise> ValueChosen;

        public event EventHandler ExercisesChanged;

        ICreateExerciseViewModel _CreateExerciseViewModel;
        public ICreateExerciseViewModel CreateExerciseViewModel
        {
            get
            {
                return _CreateExerciseViewModel;
            }
            set
            {
                _CreateExerciseViewModel = value;
                _CreateExerciseViewModel.ValueChosen += CreateExercise_ValueChosen;
            }
        }

        public SelectExerciseViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        private void CreateExercise_ValueChosen(IExercise obj)
        {
            ValueChosen?.Invoke(obj);
            navigationService.GoBack();
        }

        public IEnumerable<Exercise> Exercises
        {
            get
            {
                return Database.Table<Exercise>();
            }
        }

        const string DefaultCategory = "other";
        const string DeletedCategory = "DELETED";
        public List<ExerciseCategory> ExercisesInCategories
        {
            get
            {
                return Exercise.InExCategories(Database,
                   DefaultCategory, DeletedCategory);
            }
        }

        RelayCommand _NavigateCreateExerciseCommand;
        public RelayCommand NavigateCreateExerciseCommand
        {
            get
            {
                return _NavigateCreateExerciseCommand ??
                    (_NavigateCreateExerciseCommand = new RelayCommand(NavigateCreateExercise));
            }
        }

        public void NavigateCreateExercise()
        {
            CreateExerciseViewModel.Reset();
            navigationService.NavigateTo(
                ViewModelLocator.CreateExercisePageKey);
        }

        RelayCommand<IExercise> _SelectExerciseCommand;
        public RelayCommand<IExercise> SelectExerciseCommand
        {
            get
            {
                return _SelectExerciseCommand ??
                    (_SelectExerciseCommand = new RelayCommand<IExercise>(SelectExercise));
            }
        }

        public void SelectExercise(IExercise exercise)
        {
            ValueChosen?.Invoke(exercise);
            navigationService.GoBack();
        }

        RelayCommand<IExercise> _EditExerciseCommand;
        public RelayCommand<IExercise> EditExerciseCommand
        {
            get
            {
                return _EditExerciseCommand ??
                    (_EditExerciseCommand = new RelayCommand<IExercise>(EditExercise));
            }
        }

        public void EditExercise(IExercise exercise)
        {
            CreateExerciseViewModel.EditExercise(exercise);
            navigationService.NavigateTo(
                   ViewModelLocator.CreateExercisePageKey);
        }


        RelayCommand<IExercise> _DeleteExerciseCommand;
        public RelayCommand<IExercise> DeleteExerciseCommand
        {
            get
            {
                return _DeleteExerciseCommand ??
                    (_DeleteExerciseCommand = new RelayCommand<IExercise>(DeleteExercise));
            }
        }

        public void DeleteExercise(IExercise exercise)
        {
            DeleteExercise(exercise, null);
        }

        public void DeleteExercise(IExercise exercise, Action action_if_yes)
        {
            DialogService?.DisplayConfirmation(
                "Are you sure you want to delete the \"" +
                exercise.ToString() + " \" exercise? (this won't " +
                "have any effect on any routines that use this exercise.",
                delegate
                {
                    Database.HideDeletable((Exercise)exercise);
                    ExercisesChanged?.Invoke(this, new EventArgs());
                    action_if_yes?.Invoke();
                });
        }
    }
}
