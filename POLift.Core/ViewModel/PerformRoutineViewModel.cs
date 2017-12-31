﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POLift.Core.ViewModel
{
    using Model;
    using Service;

    public class PerformRoutineViewModel : ViewModelBase, IValueReturner<IRoutineResult>
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public IDialogService DialogService;
        public IToaster Toaster;

        public IPerformWarmupViewModel PerformWarmupViewModel;
        public ITimerViewModel TimerViewModel;
        public ICreateRoutineViewModel CreateRoutineViewModel;
        public IEditRoutineResultViewModel EditRoutineResultViewModel;

        public PerformRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }


        public event EventHandler ResultSubmittedWithoutCompleting;
        public event Action<IRoutineResult> ValueChosen;

        IRoutine _Routine;
        public IRoutine Routine {
            get
            {
                return _Routine;
            }
            set
            {
                _Routine = value;

                RoutineResult = new RoutineResult(_Routine);
                /*IRoutineResult recent_uncompleted =
                    Model.RoutineResult.MostRecentUncompleted(Database, _Routine);
                if (recent_uncompleted == null)
                {
                    RoutineResult = new RoutineResult(_Routine);
                }
                else
                {

                    //DialogService.DisplayConfirmationNeverShowAgain(
                        //)

                    RoutineResult = recent_uncompleted;
                }*/

                RefreshRoutineDetails();
            }
        }

        public const string AskForRoutineResumeKey = "ask_for_routine_resume";
        public void PromptUser()
        {
            IRoutineResult recent_uncompleted =
                Model.RoutineResult.MostRecentUncompleted(Database, Routine);

            RoutineResult = new RoutineResult(_Routine);

            if(recent_uncompleted != null &&
                (DateTime.Now - recent_uncompleted.EndTime) < TimeSpan.FromDays(1))
            {
                // there is a uncompleted routine result within the last 1 day for this routine
                // so ask user if they want to resume it
                DialogService?.DisplayConfirmationYesNoYesNeverShowAgain(
                    "You did not finish this routine on " +
                    recent_uncompleted.EndTime.ToString() +
                    ". Would you like to resume it?", AskForRoutineResumeKey,
                    delegate
                    {
                        RoutineResult = recent_uncompleted;
                    },
                    delegate
                    {
                        PromptUserForWarmupRoutine();
                    });
            }
            else
            {
                PromptUserForWarmupRoutine();
            }
        }

        const string WarmupKey = "warmup";
        public readonly string AskForWarmupKey = Service.DialogService.AskForKey(WarmupKey);
        public readonly string DefaultWarmupKey = Service.DialogService.DefaultKey(WarmupKey);

        void PromptUserForWarmupRoutine()
        {
            string exercise_name;
            if (CurrentExercise == null)
            {
                exercise_name = "";
            }
            else
            {
                string en = CurrentExercise.Name.ToLower();

                bool is_vowel = "aeiouAEIOU".IndexOf(en[0]) >= 0;

                exercise_name = (is_vowel ? "n " : " ") + en;
            }

            DialogService?.DisplayConfirmationNeverShowAgain(
                $"Would you like to do a{exercise_name} warmup routine?",
                WarmupKey, delegate
                {
                    navigationService.NavigateTo(ViewModelLocator.PerformWarmupPageKey);
                    PerformWarmupViewModel.WarmupExercise = CurrentExercise;
                });
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
                {
                    _Routine = value.Routine;
                }      

                _routine_result = value;
                _routine_result.Database = Database;

                SwitchToNextExercise();
                RefreshRoutineDetails();
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
                    System.Diagnostics.Debug.WriteLine(
                        "Call RefreshPreviousRepCountDetails");
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
                try
                {
                    if (CurrentPlateMath == null)
                    {
                        System.Diagnostics.Debug.WriteLine("No Plate Math");

                        if(CurrentExercise != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Plate Math " + 
                                CurrentExercise.PlateMathID);
                        }
                        

                        PlateMathDetails = "";
                    }
                    else
                    {
                        int WeightInput = Int32.Parse(value);
                        string plate_counts_str = CurrentPlateMath.PlateCountsToString(WeightInput);
                        PlateMathDetails = $" ({plate_counts_str})";
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
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
                System.Diagnostics.Debug.WriteLine("PlateMathDetauls = " + value);
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
                Toaster?.DisplayMessage("Weight increase!");
            }
            else
            {
                int needed_succeeds_in_a_row = CurrentExercise.ConsecutiveSetsForWeightIncrease;
                if (needed_succeeds_in_a_row > 1)
                {
                    int succeeds_in_a_row = CurrentExercise.SucceedsInARow();
                    string plur = succeeds_in_a_row > 1 ? "s" : "";

                    int needed_left = needed_succeeds_in_a_row - succeeds_in_a_row;

                    string message = "Nice! You met your rep goal for " +
                        $"{succeeds_in_a_row} set{plur} in a row. " +
                        $"You need {needed_left} more in a row to advance to the next weight";
                    Toaster?.DisplayMessage(message);
                }
            }

            if (RoutineResult.Completed)
            {
                // no more exercises
                //ReturnRoutineResult(_RoutineResult);
                ValueChosen?.Invoke(RoutineResult);
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
            if(CurrentExercise != null)
            {
                TimerViewModel.StartTimer(CurrentExercise.RestPeriodSeconds);
            }

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
                Toaster?.DisplayError(
                     "You must fill out the weight and rep count with integers");
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
                        SubmitResultFromInput();
                    }));
            }
        }

        public void ModifyRestOfRoutine()
        {
            if (this.Routine == null) return;

            int locked_sets;
            if(this.RoutineResult == null)
            {
                locked_sets = 0;
            }
            else
            {
                locked_sets = this.RoutineResult.ResultCount;
            }

            if(CreateRoutineViewModel != null)
            {
                CreateRoutineViewModel.ValueChosen += CreateRoutineViewModel_ValueChosen;
                CreateRoutineViewModel.EditRoutine(this.Routine, locked_sets);
            }
            
            navigationService.NavigateTo(ViewModelLocator.CreateRoutinePageKey);
        }

        private void CreateRoutineViewModel_ValueChosen(IRoutine new_routine)
        {
            if (Routine.Equals(new_routine)) return;

            if (this.RoutineResult.ResultCount == 0)
            {
                // routine wasn't started
                this.RoutineResult = new RoutineResult(new_routine, Database);
            }
            else
            {
                IRoutineResult old_rr = this.RoutineResult;
                this.RoutineResult = this.RoutineResult.Transform(new_routine);

                Database.HideDeletable((RoutineResult)old_rr);

                Database.Insert((RoutineResult)this.RoutineResult);
            }

            // CreateRoutineViewModel "deletes" old routine (edit operation)
            Routine = new_routine;
        }

        RelayCommand _ModifyRestOfRoutineCommand;
        public RelayCommand ModifyRestOfRoutineCommand
        {
            get
            {
                return _ModifyRestOfRoutineCommand ??
                    (_ModifyRestOfRoutineCommand = new RelayCommand(ModifyRestOfRoutine));
            }
        }

        public void IMadeAMistake()
        {
            try
            {
                EditRoutineResultViewModel.DoneEditing += EditRoutineResultViewModel_DoneEditing;
                EditRoutineResultViewModel.RoutineResult = this.RoutineResult;
                navigationService.NavigateTo(ViewModelLocator.EditRoutineResultPageKey);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void EditRoutineResultViewModel_DoneEditing()
        {
            RefreshRoutineDetails();

            EditRoutineResultViewModel.DoneEditing -= EditRoutineResultViewModel_DoneEditing;
        }

        RelayCommand _IMadeAMistakeCommand;
        public RelayCommand IMadeAMistakeCommand
        {
            get
            {
                return _IMadeAMistakeCommand ??
                    (_IMadeAMistakeCommand = new RelayCommand(IMadeAMistake));
            }
        }

    }
}
