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
    using Service;
    using Model;

    public class CreateExerciseViewModel : ViewModelBase, IValueReturner<IExercise>
    {
        public event Action<IExercise> ValueChosen;

        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public IDialogMessageService DialogService;

        public CreateExerciseViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }


        string _ExerciseNameInput;
        public string ExerciseNameInput
        {
            get
            {
                return _ExerciseNameInput;
            }
            set
            {
                Set(() => ExerciseNameInput, ref _ExerciseNameInput, value);
            }
        }

        string _RepCountInput;
        public string RepCountInput
        {
            get
            {
                return _RepCountInput;
            }
            set
            {
                Set(() => RepCountInput, ref _RepCountInput, value);
            }
        }

        string _WeightIncrementInput;
        public string WeightIncrementInput
        {
            get
            {
                return _WeightIncrementInput;
            }
            set
            {
                Set(() => WeightIncrementInput, ref _WeightIncrementInput, value);
            }
        }

        string _RestPeriodInput;
        public string RestPeriodInput
        {
            get
            {
                return _RestPeriodInput;
            }
            set
            {
                Set(() => RestPeriodInput, ref _RestPeriodInput, value);
            }
        }

        string _ConsecutiveSetsInput;
        public string ConsecutiveSetsInput
        {
            get
            {
                return _ConsecutiveSetsInput;
            }
            set
            {
                Set(() => ConsecutiveSetsInput, ref _ConsecutiveSetsInput, value);
            }
        }

        IPlateMath _PlateMath;

        public IPlateMath PlateMath
        {
            get
            {
                return _PlateMath;
            }
            set
            {
                Set(() => PlateMath, ref _PlateMath, value);
            }
        }


        Exercise CreateExerciseFromInput()
        {
            try
            {
                string name = ExerciseNameInput;
                int max_reps = Int32.Parse(RepCountInput);
                float weight_increment = Single.Parse(WeightIncrementInput);
                int rest_period_s = Int32.Parse(RestPeriodInput);
                int consecutive_sets = Int32.Parse(ConsecutiveSetsInput);

                Exercise ex = new Exercise(name, max_reps, weight_increment,
                    rest_period_s, PlateMath);
                ex.ConsecutiveSetsForWeightIncrease = consecutive_sets;
                ex.Database = this.Database;

                Database.InsertOrUndeleteAndUpdate(ex);

                //SavePreferences();

                
                return ex;
            }
            catch (FormatException)
            {
                // FormatException for int parsing

                DialogService?.DisplayTemporaryError("Numerical fields must be integers");
            }
            catch (ArgumentException ae)
            {
                // ArgumentException for Exercise constructor

                DialogService?.DisplayTemporaryError(ae.Message);
            }

            return null;
        }

        RelayCommand _CreateExerciseCommand;
        public RelayCommand CreateExerciseCommand
        {
            get
            {
                return _CreateExerciseCommand ??
                    (_CreateExerciseCommand= 
                    new RelayCommand(
                        () => {
                            Exercise result = CreateExerciseFromInput();
                            if (result != null)
                            {
                                ValueChosen?.Invoke(result);
                                navigationService.GoBack();
                            }
                        }));
            }
        }
    }
}
