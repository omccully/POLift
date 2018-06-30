using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Core.ViewModel
{
    using Service;
    using Model;
    using System.Runtime.CompilerServices;

    public class PerformWarmupViewModel : PerformBaseViewModel, IPerformWarmupViewModel
    {
        public PerformWarmupViewModel(INavigationService navigationService, IPOLDatabase database) 
            : base(navigationService, database)
        {
           
            
        }



        public override void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.RaisePropertyChanged(propertyName);

            if(propertyName == "WeightInputText")
            {
                RefreshDetails();
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
                base.CurrentExercise = value;
                WeightInputText = value.NextWeight.ToString();
                WarmupSetIndex = 0;
            }
        }

        public IExercise WarmupExercise
        {
            get
            {
                return CurrentExercise;
            }
            set
            {
                CurrentExercise = value;
            }
        }

        public int WarmupExerciseId
        {
            get
            {
                return CurrentExercise.ID;
            }
            set 
            {
                if (WarmupExercise != null && WarmupExerciseId == value) return;
                WarmupExercise = Database.ReadByID<Exercise>(value);
            }
        }

        IWarmupRoutine WarmupRoutine = Model.WarmupRoutine.Default;

        IWarmupSet NextWarmupSet
        {
            get
            {
                if (WarmupFinished) return null;
                return WarmupRoutine.WarmupSets[WarmupSetIndex];
            }
        }

        int _warmup_set_index = 0;
        int WarmupSetIndex
        {
            get
            {
                return _warmup_set_index;
            }
            set
            {
                _warmup_set_index = value;
                SetPlateMath(base.WeightInputText);
                RefreshDetails();
            }
        }

        public bool WarmupFinished
        {
            get
            {
                return WarmupSetIndex >= WarmupRoutine.WarmupSets.Count();
            }
        }

        protected override void RefreshExerciseDetails()
        {
            if (WarmupFinished)
            {
                ExerciseDetails = "Finished";
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Warmup exercise {WarmupSetIndex + 1}/{WarmupRoutine.WarmupSets.Count()}: ")
                .Append($"{NextWarmupSet.Reps} reps of {WarmupExercise.Name} at a weight of ");

            try
            {
                float weight = NextWarmupSet.GetWeight(WarmupExercise, Single.Parse(WeightInputText));
                builder.Append(weight.ToString());

                builder.Append(PlateMathDetails);
            }
            catch
            {
                builder.Append("??");
            }

            if (!String.IsNullOrEmpty(NextWarmupSet.Notes))
            {
                builder.Append($" ({NextWarmupSet.Notes})");
            }

            ExerciseDetails = builder.ToString();
        }

        protected override void SetPlateMath(float weight_input)
        {
            float warmup_weight = NextWarmupSet.GetWeight(WarmupExercise,
                weight_input);

            base.SetPlateMath(warmup_weight);
        }

        public override void RefreshRoutineDetails()
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                float weight = Single.Parse(WeightInputText);

                int i = 0;
                int last_set_index = WarmupRoutine.WarmupSets.Count() - 1;
                foreach (IWarmupSet ws in WarmupRoutine.WarmupSets)
                {
                    if (i == WarmupSetIndex)
                    {
                        builder.Append("> ");
                    }

                    builder.Append("Weight of ");
                    builder.Append(ws.GetWeight(WarmupExercise, weight));
                    builder.Append(", ");
                    builder.Append(ws.Reps.ToString());
                    builder.Append(" reps, rest ");
                    int rest_period = i == last_set_index ?
                        WarmupRoutine.GetLastRestPeriod(WarmupExercise) :
                        ws.GetRestPeriod(WarmupExercise);

                    builder.Append(rest_period.SecondsToClock());
                   // builder.Append(rest_period + "s");

                    if (i < WarmupSetIndex)
                    {
                        builder.Append(" (done)");
                    }

                    if (i != last_set_index)
                    {
                        builder.AppendLine();
                    }

                    i++;
                }

                RoutineDetails = builder.ToString();
            }
            catch
            {
                RoutineDetails = "??";
            }
        }

        public bool WarmupSetFinished(Action warmup_finished_action = null)
        {
            WarmupSetIndex++;

            if (WarmupFinished)
            {
                if(warmup_finished_action == null)
                {
                    navigationService.GoBack();

                    float weight;
                    if (Single.TryParse(this.WeightInputText, out weight))
                    {
                        ValueChosen?.Invoke(weight);
                    }
                }
                else
                {
                    warmup_finished_action();
                }

                int last_rest_period = WarmupRoutine.GetLastRestPeriod(WarmupExercise);
                TimerViewModel.StartTimer(last_rest_period);
                System.Diagnostics.Debug.WriteLine("StartTimer(" + last_rest_period);

                return true;
            }

            TimerViewModel.StartTimer(NextWarmupSet.GetRestPeriod(WarmupExercise));

            OnResultSubmittedWithoutCompleting();

            return false;
        }

        RelayCommand _SetCompletedCommand;
        public RelayCommand SetCompletedCommand
        {
            get
            {
                return _SetCompletedCommand ??
                    (_SetCompletedCommand = new RelayCommand(delegate
                    {
                        WarmupSetFinished();
                    }));
            }
        }

        public RelayCommand BackButtonCommand
        {
            get
            {
                return SkipWarmupCommand;
            }
        }

        void GoBackAction(Action go_back_action = null)
        {
            if (go_back_action == null)
            {
                navigationService.GoBack();
            }
            else
            {
                go_back_action();
            }
        }

        public void SkipWarmup(Action go_back_action = null)
        {
            if (WarmupSetIndex == 0)
            {
                GoBackAction(go_back_action);
                return;
            }

            DialogService?.DisplayConfirmationYesNoYesNeverShowAgain(
                 "Are you sure you want to end this warmup session? " +
                    " You will lose all of your progress in this warmup.",
                 "ask_for_end_warmup",
                 delegate
                 {
                     GoBackAction(go_back_action);
                 });
        }
        
        RelayCommand _SkipWarmupCommand;
        public RelayCommand SkipWarmupCommand
        {
            get
            {
                return _SkipWarmupCommand ??
                    (_SkipWarmupCommand = new RelayCommand(delegate {
                        SkipWarmup();
                    }));
            }
        }


        public const string ExerciseIdKey = "exercise_id";
        //public const string WorkingSetWeightKey = "working_set_weight";
        public const string WarmupSetIndexKey = "warmup_set_index";

        public event Action<float> ValueChosen;

        public override void RestoreState(KeyValueStorage kvs)
        {
            WarmupExerciseId = kvs.GetInteger(ExerciseIdKey, -1);

            if(WarmupExercise == null)
            {
                DialogService.DisplayAcknowledgement(
                    "Error: Invalid exercise ID (" + WarmupExerciseId + ")");
            }

            //WeightInputText = kvs.GetString(WorkingSetWeightKey, "");

            WarmupSetIndex = kvs.GetInteger(WarmupSetIndexKey, 0);

            base.RestoreState(kvs);
        }

        public override void SaveState(KeyValueStorage kvs)
        {
            InitializationState(kvs, WarmupExercise, WeightInputText);
            kvs.SetValue(WarmupSetIndexKey, WarmupSetIndex);

            
        }

        public static void InitializationState(KeyValueStorage kvs, 
            IExercise warmup_exercise, string working_set_weight)
        {
            kvs.SetValue(ExerciseIdKey, warmup_exercise.ID);

            //base.SaveState(kvs);
            kvs.SetValue(WorkingSetWeightKey, working_set_weight);
        }
    }
}
