using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using POLift.Core.Model;
using POLift.Core.Service;
using POLift.Core.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class PerformRoutineController : DatabaseController, IValueReturner<IRoutineResult>
    {
        public event Action<IRoutineResult> ValueChosen;

        public IRoutine Routine { get; set; }

        IRoutineResult _routine_result;
        IRoutineResult _RoutineResult
        {
            get
            {
                return _routine_result;
            }
            set
            {
                _routine_result = value;
                _routine_result.Database = Database;

                GetNextExerciseAndWeight();
            }
        }
        IExercise CurrentExercise;

        IPlateMath CurrentPlateMath
        {
            get
            {
                if (CurrentExercise == null) return null;
                return PlateMath.PlateMathTypes[CurrentExercise.PlateMathID];
            }
        }

        public PerformRoutineController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ReportResultButton.TouchUpInside += ReportResultButton_TouchUpInside;
            WeightTextField.ValueChanged += WeightTextField_ValueChanged;

            IRoutineResult recent_uncompleted = 
               RoutineResult.MostRecentUncompleted(Database, Routine);
            if(recent_uncompleted == null)
            {
                _RoutineResult = new RoutineResult(Routine);
            }
            else
            {
                _RoutineResult = recent_uncompleted;
            }

            RefreshGUI();
        }

        private void WeightTextField_ValueChanged(object sender, EventArgs e)
        {
            Console.WriteLine("WeightTextField_ValueChanged");
            try
            {
                Console.WriteLine($"CurrentExercise.PlateMathID={CurrentExercise.PlateMathID}");
                if (CurrentPlateMath == null)
                {
                    PlateMathLabel.Text = "";
                }
                else
                {
                    int WeightInput = Int32.Parse(WeightTextField.Text);
                    string plate_counts_str = CurrentPlateMath.PlateCountsToString(WeightInput);
                    PlateMathLabel.Text = $" ({plate_counts_str})";
                }
            }
            catch (FormatException)
            {

            }
        }

        private void ReportResultButton_TouchUpInside(object sender, EventArgs e)
        {
            // user submitted a result for this CurrentExercise

            // get number of reps and clear the text box
            int reps = 0;
            float weight = 0;
            try
            {
                weight = Int32.Parse(WeightTextField.Text);
                reps = Int32.Parse(RepCountTextField.Text);
            }
            catch (FormatException)
            {
                // "You must fill out the weight and rep count with integers",
                return;
            }

            RepCountTextField.Text = "";

            if (ReportExerciseResult(weight, reps))
            {
                // if there's more exercises, try to show an ad
                //TryShowFullScreenAd();
            }
        }

        bool ReportExerciseResult(float weight, int reps)
        {
            // report the exercise result
            ExerciseResult ex_result =
                new ExerciseResult(CurrentExercise, weight, reps);
            ex_result.Database = Database;
            Database.Insert(ex_result);
            _RoutineResult.ReportExerciseResult(ex_result);

            // insert or update the routine result after EVERY new result
            // just in case the app crashes or something
            Database.InsertOrUpdateByID(_RoutineResult);

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

            if (_RoutineResult.Completed)
            {
                // no more exercises
                ReturnRoutineResult(_RoutineResult);
                //StaticTimer.StopTimer();
                //CancelTimerNotification();
                return false;
            }

            // update Weight and CurrentExercise
            GetNextExerciseAndWeight();
            // rest period is based on the NEXT exercise's rest period

            //StartRestPeriod();

            //PromptUserForRating();

            return true;
        }

        void ReturnRoutineResult(IRoutineResult routine_result)
        {
            ValueChosen?.Invoke(routine_result);
            NavigationController.PopViewController(true);
        }

        void GetNextExerciseAndWeight()
        {
            // get next exercise
            CurrentExercise = _RoutineResult.NextExercise;

            if (CurrentExercise != null)
            {
                WeightTextField.Text = CurrentExercise.NextWeight.ToString();
            }

            RefreshGUI();
        }

        void RefreshGUI()
        {
            RefreshRoutineDetails();
            ExerciseDetailsLabel.Text = CurrentExercise.ToString();
        }

        void RefreshRoutineDetails()
        {
            if (_RoutineResult != null)
            {
                RoutineDetailsLabel.Text = _RoutineResult.ShortDetails;
            }
            else if (Routine != null)
            {
                RoutineDetailsLabel.Text = Routine.ToString();
            }
            else
            {
                RoutineDetailsLabel.Text = "?";
            }
        }

        void RefreshExerciseDetails()
        {
            if (CurrentExercise != null)
            {
                ExerciseDetailsLabel.Text = $"Exercise {_RoutineResult.ResultCount + 1}/"
                    + $"{ _RoutineResult.ExerciseCount}: "
                    + CurrentExercise.ShortDetails;

            }
            else if (_RoutineResult != null && _RoutineResult.Completed)
            {
                ExerciseDetailsLabel.Text = "Routine completed" + (_RoutineResult == null ? "" : "!");
            }
            else
            {
                ExerciseDetailsLabel.Text = "Pending";
            }

            IEnumerable<ExerciseResult> previous_ers = Database.Table<ExerciseResult>()
                .Where(er => er.ExerciseID == CurrentExercise?.ID &&
                    er.Time < _RoutineResult.StartTime)
                .TakeLast(3);
            if (previous_ers.Count() == 0)
            {
                RepDetailsLabel.Text = "";
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

                RepDetailsLabel.Text = s;
            }

        }

        
    }
}