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

        public event Action<IExercise> ValueChosen;

        public SelectExerciseViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;

            ViewModelLocator.Default.CreateExercise.ValueChosen += CreateExercise_ValueChosen;
        }

        private void CreateExercise_ValueChosen(IExercise obj)
        {
            ValueChosen?.Invoke(obj);
        }

        RelayCommand _CreateExerciseCommand;
        public RelayCommand CreateExerciseCommand
        {
            get
            {
                return _CreateExerciseCommand ??
                    (_CreateExerciseCommand =
                    new RelayCommand(
                        () => navigationService.NavigateTo(
                                ViewModelLocator.CreateExercisePageKey)));
            }
        }

        public RelayCommand SelectExerciseCommand(IExercise exercise)
        {
            return new RelayCommand(
                    () => ValueChosen?.Invoke(exercise));
        }
    }
}
