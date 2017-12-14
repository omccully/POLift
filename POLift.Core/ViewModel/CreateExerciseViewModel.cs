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

        public KeyValueStorage KeyValueStorage;
        public IToaster Toaster;

        public const string MaxRepsStorageKey = "create_exercise_max_reps";
        public const string WeightIncrementStorageKey = "create_exercise_weight_increment";
        public const string RestPeriodStorageKey = "create_exercise_rest_period_seconds";
        public const string ConsecutiveSetsStorageKey = "consecutive_sets_for_weight_increase";
        public const string ExerciseCreatedSinceLastDifficultyRegenerationStorageKey =
            "exercise_created_since_last_difficulty_regeneration";

        const string DefaultMaxReps = "8";
        const string DefaultWeightIncrement = "5";
        const string DefaultRestPeriod = "150";
        const string DefaultConsecutiveSets = "1";

        public CreateExerciseViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public void Reset()
        {
            ExerciseNameInput = "";

            if(KeyValueStorage == null)
            {
                RepCountInput = DefaultMaxReps;
                WeightIncrementInput = DefaultWeightIncrement;
                RestPeriodInput = DefaultRestPeriod;
                ConsecutiveSetsInput = DefaultConsecutiveSets;
            }
            else
            {
                LoadPreferences();
            }

            PlateMath = null;
        }

        void LoadPreferences()
        {
            RepCountInput = KeyValueStorage.GetString(MaxRepsStorageKey, DefaultMaxReps);
            WeightIncrementInput = KeyValueStorage.GetString(WeightIncrementStorageKey, DefaultWeightIncrement);
            RestPeriodInput = KeyValueStorage.GetString(RestPeriodStorageKey, DefaultRestPeriod);
            ConsecutiveSetsInput = KeyValueStorage.GetString(ConsecutiveSetsStorageKey, DefaultConsecutiveSets);
        }

        void SavePreferences()
        {
            KeyValueStorage
                .SetValue(MaxRepsStorageKey, RepCountInput)
                .SetValue(WeightIncrementStorageKey, WeightIncrementInput)
                .SetValue(RestPeriodStorageKey, RestPeriodInput)
                .SetValue(ConsecutiveSetsStorageKey, ConsecutiveSetsInput);
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

                SavePreferences();

                
                return ex;
            }
            catch (FormatException)
            {
                // FormatException for int parsing

                Toaster?.DisplayError("Numerical fields must be integers");
            }
            catch (ArgumentException ae)
            {
                // ArgumentException for Exercise constructor

                Toaster?.DisplayError(ae.Message);   
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
