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
    using Helpers;

    public class PerformRoutineViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public PerformRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }


        public event EventHandler ResultSubmittedWithoutCompleting;

        IRoutine _Routine;
        public IRoutine Routine {
            get
            {
                return _Routine;
            }
            set
            {
                _Routine = value;
                IRoutineResult recent_uncompleted =
                    Model.RoutineResult.MostRecentUncompleted(Database, _Routine);
                if (recent_uncompleted == null)
                {
                    RoutineResult = new RoutineResult(_Routine);
                }
                else
                {
                    RoutineResult = recent_uncompleted;
                }

                RefreshRoutineDetails();
            }
        }

        IRoutineResult _routine_result;
        IRoutineResult RoutineResult
        {
            get
            {
                return _routine_result;
            }
            set
            {
                if (value.RoutineID != Routine.ID)
                    throw new ArgumentException("RoutineResult is not for Routine");

                _routine_result = value;
                _routine_result.Database = Database;

                SwitchToNextExercise();
            }
        }

        IExercise _CurrentExercise;
        IExercise CurrentExercise
        {
            get
            {
                return _CurrentExercise;
            }
            set
            {
                bool is_different_from_previous = _CurrentExercise != value;

                _CurrentExercise = value;

                ResetWeightInput();

                RefreshExerciseDetails();

                if (is_different_from_previous)
                {
                    RefreshPreviousRepCountDetails();
                }
            }
        }

        public void ResetWeightInput()
        {
            if (CurrentExercise != null)
            {
                WeightInputText = CurrentExercise.NextWeight.ToString();
            }
        }

        IPlateMath CurrentPlateMath
        {
            get
            {
                if (CurrentExercise == null) return null;
                return PlateMath.PlateMathTypes[CurrentExercise.PlateMathID];
            }
        }

        string _WeightInputText;
        public string WeightInputText
        {
            get
            {
                return _WeightInputText;
            }
            set
            {
                System.Diagnostics.Debug.WriteLine($"updating WeightInputText={value}");

                try
                {
                    if (CurrentPlateMath == null)
                    {
                        PlateMathDetails = "";
                    }
                    else
                    {
                        int WeightInput = Int32.Parse(value);
                        string plate_counts_str = CurrentPlateMath.PlateCountsToString(WeightInput);
                        PlateMathDetails = $" ({plate_counts_str})";
                    }
                }
                catch
                {

                }

                Set(() => WeightInputText, ref _WeightInputText, value);
            }
        }

        string _RepsInputText;
        public string RepsInputText
        {
            get
            {
                return _RepsInputText;
            }
            set
            {
                System.Diagnostics.Debug.WriteLine($"updating RepsInputText={value}");
                Set(() => RepsInputText, ref _RepsInputText, value);
            }
        }

        void SwitchToNextExercise()
        {
            // get next exercise
            CurrentExercise = RoutineResult.NextExercise;
        }

        string _PlateMathDetails;
        public string PlateMathDetails
        {
            get
            {
                return _PlateMathDetails;
            }
            set
            {
                System.Diagnostics.Debug.WriteLine($"updating PlateMathDetails={value}");
                Set(ref _PlateMathDetails, value);
            }
        }

        string _RoutineDetails;
        public string RoutineDetails
        {
            get
            {
                return _RoutineDetails;
            }
            set
            {
                Set(ref _RoutineDetails, value);
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
                Set(ref _ExerciseDetails, value);
            }
        }

        string _RepDetails;
        public string RepDetails
        {
            get
            {
                return _RepDetails;
            }
            set
            {
                Set(ref _RepDetails, value);
            }
        }

        void RefreshDetails()
        {
            RefreshRoutineDetails();
            RefreshExerciseDetails();
        }

        void RefreshRoutineDetails()
        {
            if (RoutineResult != null)
            {
                RoutineDetails = RoutineResult.ShortDetails;
            }
            else if (Routine != null)
            {
                RoutineDetails = Routine.ToString();
            }
            else
            {
                RoutineDetails = "?";
            }
        }

        void RefreshExerciseDetails()
        {
            if (CurrentExercise != null)
            {
                ExerciseDetails = $"Exercise {RoutineResult.ResultCount + 1}/"
                    + $"{ RoutineResult.ExerciseCount}: "
                    + CurrentExercise.ShortDetails;
            }
            else if (RoutineResult != null && RoutineResult.Completed)
            {
                ExerciseDetails = "Routine completed" + (RoutineResult == null ? "" : "!");
            }
            else
            {
                ExerciseDetails = "Pending";
            }
        }


        void RefreshPreviousRepCountDetails()
        {
            IEnumerable<ExerciseResult> previous_ers = Database.Table<ExerciseResult>()
                .Where(er => er.ExerciseID == CurrentExercise?.ID &&
                    er.Time < RoutineResult.StartTime)
                .TakeLastEx(3);

            if (previous_ers.Count() == 0)
            {
                RepDetails = "";
            }
            else
            {
                ExerciseResult first = previous_ers.First();
                ExerciseResult previous = first;
                string s = $" (prev: {first.Weight}x{first.RepCount}";

                foreach (ExerciseResult er in previous_ers.Skip(1))
                {
                    if (er.Weight == previous.Weight)
                    {
                        s += $", x{er.RepCount}";
                    }
                    else
                    {
                        s += $", {er.Weight}x{er.RepCount}";
                    }


                    previous = er;
                }
                s += ")";

                RepDetails = s;
            }
        }

        bool ReportExerciseResult(float weight, int reps)
        {
            // report the exercise result
            ExerciseResult ex_result =
                new ExerciseResult(CurrentExercise, weight, reps);
            ex_result.Database = Database;
            Database.Insert(ex_result);
            RoutineResult.ReportExerciseResult(ex_result);

            // insert or update the routine result after EVERY new result
            // just in case the app crashes or something
            Database.InsertOrUpdateByID(RoutineResult);

            //if(reps >= CurrentExercise.MaxRepCount)
            if (CurrentExercise.NextWeight > weight)
            {
                // "Weight increase!"
            }
            else
            {
                int needed_succeeds_in_a_row = CurrentExercise.ConsecutiveSetsForWeightIncrease;
                if (needed_succeeds_in_a_row > 1)
                {
                    int succeeds_in_a_row = CurrentExercise.SucceedsInARow();
                    string plur = succeeds_in_a_row > 1 ? "s" : "";

                    int needed_left = needed_succeeds_in_a_row - succeeds_in_a_row;

                    //string message = "Nice! You met your rep goal for " +
                    //    $"{succeeds_in_a_row} set{plur} in a row. " +
                    //    $"You need {needed_left} more in a row to advance to the next weight";

                }
            }

            if (RoutineResult.Completed)
            {
                // no more exercises
                //ReturnRoutineResult(_RoutineResult);

                navigationService.GoBack();

                //StaticTimer.StopTimer();
                //CancelTimerNotification();
                return false;
            }

            // update Weight and CurrentExercise
            SwitchToNextExercise();

            RefreshRoutineDetails();

            // rest period is based on the NEXT exercise's rest period

            //StartRestPeriod();

            //PromptUserForRating();

            return true;
        }



        void SubmitResultFromInput()
        {
            // user submitted a result for this CurrentExercise

            // get number of reps and clear the text box
            int reps = 0;
            float weight = 0;
            try
            {
                weight = Int32.Parse(WeightInputText);
                reps = Int32.Parse(RepsInputText);
            }
            catch (FormatException)
            {
                // "You must fill out the weight and rep count with integers",
                return;
            }


            RepsInputText = "";

            if (ReportExerciseResult(weight, reps))
            {
                // if there's more exercises, try to show an ad
                //TryShowFullScreenAd();
                ResultSubmittedWithoutCompleting?.Invoke(this, new EventArgs());
            }
        }

        RelayCommand _SubmitResultCommand;
        public RelayCommand SubmitResultCommand
        {
            get
            {
                return _SubmitResultCommand ??
                    (_SubmitResultCommand = new RelayCommand(delegate
                    {
                        System.Diagnostics.Debug.WriteLine($"ar1234a weight={WeightInputText} rep={RepsInputText}");
                        try
                        {
                            SubmitResultFromInput();
                        }
                        catch(Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.ToString());

                            throw e;
                        }
                    }));
            }
        }
    }
}
