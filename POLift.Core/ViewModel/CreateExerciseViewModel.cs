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

    public class CreateExerciseViewModel : ViewModelBase, ICreateExerciseViewModel, IValueReturner<IExercise>
    {
        public event Action<IExercise> ValueChosen;

        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public DialogService DialogService;
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

        const string TutorialSelectExerciseKey = "tutorial_select_exercise";
        public void InfoUser()
        {
            DialogService.DisplayAcknowledgementOnce("POLift sets itself apart from other apps by " +
                "automatically starting a rest timer and " +
                "automatically increasing the weight once you reach a preset goal for a certain weight. " +
                "For example, an exercise can be configured for increasing the weight when you get 5 reps 5 times in a row (for StrongLifts 5x5) " +
                "or at 6 reps for the first time (for Bigger Leaner Stronger program). " +
                "You set these rules yourself when you create an exercise.", TutorialSelectExerciseKey);
        }

        public void EditExercise(IExercise exercise)
        {
            // editing exercise does not affect original
            // unless ONLY platemath was changed. 

            Title = EditExerciseTitle;
            SubmitButtonText = EditExerciseButtonText;

            ExerciseNameInput = exercise.Name;
            RepCountInput = exercise.MaxRepCount.ToString();
            WeightIncrementInput = exercise.WeightIncrement.ToString();
            RestPeriodInput = exercise.RestPeriodSeconds.ToString();
            ConsecutiveSetsInput = exercise.ConsecutiveSetsForWeightIncrease.ToString();

            PlateMath = exercise.PlateMath;
        }

        public void Reset()
        {
            Title = CreateExerciseTitle;
            SubmitButtonText = CreateExerciseButtonText;
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

        const string CreateExerciseTitle = "Create Exercise";
        const string EditExerciseTitle = "Edit Exercise";
        string _Title = CreateExerciseTitle;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                Set(() => Title, ref _Title, value);
            }
        }

        const string CreateExerciseButtonText = "Create exercise";
        const string EditExerciseButtonText = "Apply changes";
        string _SubmitButtonText = CreateExerciseButtonText;
        public string SubmitButtonText
        {
            get
            {
                return _SubmitButtonText;
            }
            set
            {
                Set(() => SubmitButtonText, ref _SubmitButtonText, value);
            }
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
                System.Diagnostics.Debug.WriteLine("ExerciseNameInput");
                Set(() => ExerciseNameInput, ref _ExerciseNameInput, value);
                UpdateExerciseDetails();
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
                System.Diagnostics.Debug.WriteLine("RepCountInput");
                Set(() => RepCountInput, ref _RepCountInput, value);
                UpdateExerciseDetails();
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
                System.Diagnostics.Debug.WriteLine("RepCountInput");
                Set(() => WeightIncrementInput, ref _WeightIncrementInput, value);
                UpdateExerciseDetails();
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
                System.Diagnostics.Debug.WriteLine("RestPeriodInput");
                Set(() => RestPeriodInput, ref _RestPeriodInput, value);
                UpdateExerciseDetails();
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
                System.Diagnostics.Debug.WriteLine("ConsecutiveSetsInput");
                Set(() => ConsecutiveSetsInput, ref _ConsecutiveSetsInput, value);
                UpdateExerciseDetails();
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

        public int PlateMathID
        {
            get
            {
                int index = Array.IndexOf(Service.PlateMath.PlateMathTypes, PlateMath);
                if (index == -1) return 0;
                return index;
            }
            set
            {
                PlateMath = Service.PlateMath.PlateMathTypes[value];
            }
        }

        string _ExerciseDetails;
        public string ExerciseDetails
        {
            get
            {
                return _ExerciseDetails;
            }
            set
            {
                Set(() => ExerciseDetails, ref _ExerciseDetails, value);
            }
        }

        public Exercise CreateExerciseFromInput()
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
                // TODO: fix issue with editing PlateMath wiping out the Category
                // similarly, make sure usage is set properly,

                System.Diagnostics.Debug.WriteLine("Saved exercise with PM ID " + ex.PlateMathID);

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

        void UpdateExerciseDetails()
        {
            // You will as many reps of <exercise name>
            // as you can for each set. If you get <max reps> reps,
            // you will increase the weight by <weight increment>

            int cs = -1;
            bool cs_success = Int32.TryParse(ConsecutiveSetsInput, out cs);

            StringBuilder builder = new StringBuilder();

            builder.Append("Explanation of your input: ");

            if (!cs_success || cs >= 1)
            {
                builder.Append("You will try to get as many reps of ")
                .Append(EnsureString(ExerciseNameInput, "<exercise name>"))
                .Append(" as you can for each set. If you get ")
                .Append(EnsureInt(RepCountInput, "<reps>"))
                .Append(" reps");

                if (cs > 1)
                {
                    builder.Append(" for ")
                        .Append(EnsureInt(ConsecutiveSetsInput, "<consecutive sets>"))
                        .Append(" sets in a row");
                }

                builder.Append(", you will increase the weight by ")
                   .Append(EnsureFloat(WeightIncrementInput, "<weight increment>"))
                   .Append(" for your next set. ");
            }
            else if (cs == 0)
            {
                builder.Append("You will try to get ")
                    .Append(EnsureInt(RepCountInput, "<reps>"))
                    .Append(" reps for each set. You will increase the weight manually ")
                    .Append("(it's normally recommended to let the app increase weight automatically")
                    .Append(" using setting \"consecutive sets\" to a number greater than 0). ");
            }


            builder.Append("You will rest for ")
                 .Append(EnsureInt(RestPeriodInput, "<rest period seconds>"))
                 .Append(" seconds in between sets.");

            ExerciseDetails = builder.ToString();
        }

        string EnsureString(string input_text, string fail_text = "?")
        {
            return String.IsNullOrWhiteSpace(input_text) ? fail_text : input_text;
        }

        string EnsureInt(string input_text, string fail_text = "?")
        {
            try
            {
                return Int32.Parse(input_text).ToString();
            }
            catch
            {
                return fail_text;
            }
        }

        string EnsureFloat(string input_text, string fail_text = "?")
        {
            try
            {
                return Single.Parse(input_text).ToString();
            }
            catch
            {
                return fail_text;
            }
        }

        public void Help()
        {
            DialogService.DisplayAcknowledgement(ExerciseDetails.Replace(": ", ":\n"));
        }

        RelayCommand _HelpCommand;
        public RelayCommand HelpCommand
        {
            get
            {
                return _HelpCommand ??
                    (_HelpCommand = new RelayCommand(Help));
            }
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
                                navigationService.GoBack();
                                ValueChosen?.Invoke(result);
                            }
                        }));
            }
        }
    }
}
