using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POLift.Core.ViewModel
{
    using Helpers;
    using Model;
    using Service;

    public class PerformRoutineViewModel : PerformBaseViewModel, IValueReturner<IRoutineResult>
    {
        public IToaster Toaster;

        public IPerformWarmupViewModel PerformWarmupViewModel;
        public ICreateRoutineViewModel CreateRoutineViewModel;
        public IEditRoutineResultViewModel EditRoutineResultViewModel;
        public ICreateExerciseViewModel CreateExerciseViewModel;

        public PerformRoutineViewModel(INavigationService navigationService, IPOLDatabase database)
            : base(navigationService, database)
        {

        }

        public event EventHandler ResultSubmittedWithoutCompleting;
        public event Action<IRoutineResult> ValueChosen;

        IRoutine _Routine;
        public IRoutine Routine
        {
            get
            {
                return _Routine;
            }
            set
            {
                _Routine = value;

                RoutineResult = new RoutineResult(_Routine);

                RefreshRoutineDetails();
            }
        }

        public int RoutineId
        {
            get
            {
                return Routine.ID;
            }
            set
            {
                Routine = Database.ReadByID<Routine>(value);
            }
        }

        public const string AskForRoutineResumeKey = "ask_for_routine_resume";
        public void PromptUser()
        {
            IRoutineResult recent_uncompleted =
                Model.RoutineResult.MostRecentUncompleted(Database, Routine);

            RoutineResult = new RoutineResult(_Routine);

            if (recent_uncompleted != null &&
                (DateTime.Now - recent_uncompleted.EndTime) < TimeSpan.FromDays(1))
            {
                PromptUserToResumeRoutine(recent_uncompleted);
            }
            else
            {
                PromptUserForWarmupRoutine();
            }
        }

        void PromptUserToResumeRoutine(IRoutineResult recent_uncompleted)
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

        public Action StartWarmup = null;

        const string WarmupKey = "warmup";
        public readonly string AskForWarmupKey = Service.DialogService.AskForKey(WarmupKey);
        public readonly string DefaultWarmupKey = Service.DialogService.DefaultKey(WarmupKey);

        void PromptUserForWarmupRoutine()
        {
            System.Diagnostics.Debug.WriteLine("PromptUserForWarmupRoutine");
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

            DialogService.DisplayConfirmationNeverShowAgain(
                $"Would you like to do a{exercise_name} warmup routine?",
                WarmupKey, delegate
                {
                    if(StartWarmup == null)
                    {
                        navigationService.NavigateTo(ViewModelLocator.PerformWarmupPageKey);
                        PerformWarmupViewModel.WarmupExercise = CurrentExercise;
                    }
                    else
                    {
                        StartWarmup();
                    }
                });
            System.Diagnostics.Debug.WriteLine("PromptUserForWarmupRoutine end" + DialogService);
        }

        IRoutineResult _routine_result;
        public IRoutineResult RoutineResult
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


        public override IExercise CurrentExercise
        {
            get
            {
                return base.CurrentExercise;
            }
            set
            {
                bool is_different_from_previous =
                    base.CurrentExercise != value;

                base.CurrentExercise = value;

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

        public override void RefreshRoutineDetails()
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

        protected override void RefreshExerciseDetails()
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

        void SyncTimerBasedOnLastExerciseResult()
        {
            if (this.RoutineResult == null) return;

            IExerciseResult last_ex_result = this.RoutineResult
                .ExerciseResults.LastOrDefault();

            if (last_ex_result == null) return;

            int rest_period_seconds = last_ex_result.Exercise.RestPeriodSeconds;
            TimeSpan rest_period_span = TimeSpan.FromSeconds(rest_period_seconds);
            TimeSpan time_since_last_exercise = (DateTime.Now - last_ex_result.Time);
            if (time_since_last_exercise < rest_period_span)
            {
                int remaining_timer = (int)(rest_period_seconds - time_since_last_exercise.TotalSeconds);
                TimerViewModel.StartTimer(remaining_timer);
            }
        }

        public void PromptUserForRating(Action rating_action)
        {
            const string ask_for_rating_pref_key = "ask_for_rating";

            if (RoutineResult.ResultCount != 1) return;

            int rr_count = Database.Table<RoutineResult>().Count();

            if (rr_count != 10 && rr_count < 15) return;

            DialogService.DisplayConfirmationYesNotNowNever(
                "Thank you for using POLift. Would you like to " +
                "rate this app in the Google Play store? ",
                ask_for_rating_pref_key, delegate
                {
                    rating_action?.Invoke();
                });

            if (rr_count > 15)
            {
                DialogService.KeyValueStorage
                    .SetValue(ask_for_rating_pref_key, false);
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
            if (CurrentExercise != null)
            {
                StartTimer();
            }

            //PromptUserForRating();

            return true;
        }

        public void StartTimer()
        {
            TimerViewModel.StartTimer(CurrentExercise.RestPeriodSeconds);
        }

        public void SubmitResultFromInput()
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
            if (this.RoutineResult == null)
            {
                locked_sets = 0;
            }
            else
            {
                locked_sets = this.RoutineResult.ResultCount;
            }

            if (CreateRoutineViewModel != null)
            {
                CreateRoutineViewModel.ValueChosen += CreateRoutineViewModel_ValueChosen;
                CreateRoutineViewModel.EditRoutine(this.Routine, locked_sets);
            }

            navigationService.NavigateTo(ViewModelLocator.CreateRoutinePageKey);
        }


        public void CreateRoutineViewModel_ValueChosen(int new_routine_id)
        {
            TranslateToRoutine(Database.ReadByID<Routine>(new_routine_id));
        }

        public void CreateRoutineViewModel_ValueChosen(IRoutine new_routine)
        {
            TranslateToRoutine(new_routine);
        }

        void TranslateToRoutine(IRoutine new_routine, bool delete_old_routine = false,
            bool safe = true)
        {
            if (Routine.Equals(new_routine)) return;

            // CreateRoutineViewModel "deletes" old routine (edit operation)
            if (delete_old_routine)
            {
                Database.HideDeletable((Routine)Routine);
            }

            if (this.RoutineResult.ResultCount == 0)
            {
                // routine wasn't started
                this.RoutineResult = new RoutineResult(new_routine, Database);
                System.Diagnostics.Debug.WriteLine("Routine wasn't started yet, so starting new one with new_routine");
            }
            else
            {
                IRoutineResult old_rr = this.RoutineResult;
                this.RoutineResult = this.RoutineResult.Transform(new_routine, safe);

                Database.HideDeletable((RoutineResult)old_rr);

                Database.Insert((RoutineResult)this.RoutineResult);
            }

            // this isn't needed because setting RoutineResult also sets Routine
            //Routine = new_routine;
        }

        public void EditThisExercise()
        {
            if (this.Routine == null) return;

            if (CreateExerciseViewModel != null)
            {
                CreateExerciseViewModel.ValueChosen += CreateExercise_ValueChosen;
                CreateExerciseViewModel.EditExercise(this.CurrentExercise);
            }

            navigationService.NavigateTo(ViewModelLocator.CreateExercisePageKey);
        }

        public void CreateExercise_ValueChosen(int exercise_id)
        {
            CreateExercise_ValueChosen(Database.ReadByID<Exercise>(exercise_id));
        }

        private void CreateExercise_ValueChosen(IExercise obj)
        {
            const string ShowReplaceExercisesWarning = "show_replace_exercises_warning";
            System.Diagnostics.Debug.WriteLine("PerformRoutineViewModel.CreateExercise_ValueChosen");
            if (navigationService.CurrentPageKey !=
                ViewModelLocator.PerformRoutinePageKey)
            {
                System.Diagnostics.Debug.WriteLine("PerformRoutineViewModel.CreateExercise_ValueChosen skipped; CurrentPageKey = "
                     + navigationService.CurrentPageKey);
                return;
            }

            if (CurrentExercise.Name != obj.Name)
            {
                DialogService.DisplayConfirmationNeverShowAgain(
                    $"You are about to replace ALL of the \"{CurrentExercise.Name}\" exercises" +
                    $" in this routine with {obj.Name}", ShowReplaceExercisesWarning,
                    delegate
                    {
                        ReplaceAllOfCurrentExerciseWith(obj);
                    });
            }
            else
            {
                ReplaceAllOfCurrentExerciseWith(obj);
            }
        }

        void ReplaceAllOfCurrentExerciseWith(IExercise obj)
        {
            try
            {
                IExerciseSets current_es = RoutineResult.CurrentExerciseSets;

                ExerciseSets new_es = new ExerciseSets(obj.ID,
                    current_es.SetCount, current_es.Database);

                Database.InsertOrUpdateNoID(new_es);

                string new_id_str = Helpers.ToIDIntegers(Routine.ExerciseSetIDs)
                    .Select(old_es_id =>
                        old_es_id == current_es.ID ? new_es.ID : old_es_id
                    ).ToIDString();

                Routine new_routine = new Routine(Routine.Name, new_id_str);
                new_routine.Database = Routine.Database;
                Database.InsertOrUpdateNoID(new_routine);

                TranslateToRoutine(new_routine, true, false);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                throw e;
            }
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

        RelayCommand _EditThisExerciseCommand;
        public RelayCommand EditThisExerciseCommand
        {
            get
            {
                return _EditThisExerciseCommand ??
                    (_EditThisExerciseCommand = new RelayCommand(EditThisExercise));
            }
        }


        public void IMadeAMistake(Action navigate_action = null)
        {
            try
            {
                if (RoutineResult == null)
                {
                    Toaster.DisplayError("Error");
                    return;
                }
                if (RoutineResult.ResultCount == 0)
                {
                    Toaster.DisplayError("You have not started this routine yet");
                    return;
                }

                if (navigate_action == null)
                {
                    EditRoutineResultViewModel.DoneEditing += EditRoutineResultViewModel_DoneEditing;
                    EditRoutineResultViewModel.RoutineResult = this.RoutineResult;
                    navigationService.NavigateTo(ViewModelLocator.EditRoutineResultPageKey);
                }
                else
                {
                    navigate_action();
                }
            }
            catch (Exception e)
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
                    (_IMadeAMistakeCommand = new RelayCommand(delegate {
                        IMadeAMistake();
                    }));
            }
        }

        public const string RoutineIdKey = "routine_id";
        public const string ResumeRoutineResultIdKey = "resume_routine_result_id";
        public const string WarmupPromptedKey = "warmup_prompted";

        public void RestoreState(KeyValueStorage kvs)
        {
            // set Routine
            RoutineId = kvs.GetInteger(RoutineIdKey, -1);

            IRoutineResult recent_uncompleted = Model.RoutineResult
                .MostRecentUncompleted(Database, Routine);

            int resume_routine_result_id = kvs.GetInteger(ResumeRoutineResultIdKey);

            bool warmup_prompted = kvs.GetBoolean(WarmupPromptedKey, false);

            /*if (resume_routine_result_id == 0)
            {
                System.Diagnostics.Debug.WriteLine("a");
                // a routine result was started but has no contents
                this.RoutineResult = new RoutineResult(Routine);
            }
            else */
            
            if (recent_uncompleted != null && recent_uncompleted.ID == resume_routine_result_id)
            {
                System.Diagnostics.Debug.WriteLine("b");
                // only restore rr state if it's the most recent rr for the routine
                this.RoutineResult = recent_uncompleted;
            }
            else
            {
                // FIRST LAUNCH

                System.Diagnostics.Debug.WriteLine("c");
                // there was no saved state

                if (recent_uncompleted == null ||
                    (DateTime.Now - recent_uncompleted.EndTime) > TimeSpan.FromDays(1))
                {
                    // if there is no recent uncompleted routine result for this routine 
                    // OR
                    // if the most recent uncompleted routine result is more than a day ago, 
                    // just start a new one without asking

                    this.RoutineResult = new RoutineResult(Routine);
                    if (!warmup_prompted /*&& previous_activity_depth == 0*/)
                    {
                        PromptUserForWarmupRoutine();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("d");
                    // there is a uncompleted routine result within the last 1 day for this routine
                    // so ask user if they want to resume it

                    // if no, prompts user for warmup as well
                    PromptUserToResumeRoutine(recent_uncompleted);
                }
            }
        }

        public void SaveState(KeyValueStorage kvs)
        {
            if(this.RoutineResult != null)
                kvs.SetValue(ResumeRoutineResultIdKey, this.RoutineResult.ID);
            
            if(this.Routine != null)
                kvs.SetValue(RoutineIdKey, this.Routine.ID);

            kvs.SetValue(WarmupPromptedKey, true);
        }
    }
}
