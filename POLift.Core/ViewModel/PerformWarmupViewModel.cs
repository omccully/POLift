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

    public class PerformWarmupViewModel : ViewModelBase, IPerformWarmupViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public IDialogService DialogService;
        public ITimerViewModel TimerViewModel;

        public PerformWarmupViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        IExercise _WarmupExercise;
        public IExercise WarmupExercise
        {
            get
            {
                return _WarmupExercise;
            }
            set
            {
                _WarmupExercise = value;
                WeightInputText = value.NextWeight.ToString();
                WarmupSetIndex = 0;
            }
        }

        IWarmupSet[] WarmupSets = WarmupSet.Default;

        IWarmupSet NextWarmupSet
        {
            get
            {
                if (WarmupFinished) return null;
                return WarmupSets[WarmupSetIndex];
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
                RefreshWarmupDetails();
            }
        }

        bool WarmupFinished
        {
            get
            {
                return WarmupSetIndex >= WarmupSets.Length;
            }
        }

        IPlateMath CurrentPlateMath
        {
            get
            {
                if (WarmupExercise == null) return null;
                return PlateMath.PlateMathTypes[WarmupExercise.PlateMathID];
            }
        }

        void RefreshWarmupDetails()
        {
            RefreshCurrentWarmupDetails();

            RefreshFullWarmupDetails();
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

        string _WeightInputText;
        public string WeightInputText
        {
            get
            {
                return _WeightInputText;
            }
            set
            {
                Set(() => WeightInputText, ref _WeightInputText, value);
                RefreshWarmupDetails();
            }
        }

        void RefreshCurrentWarmupDetails()
        {
            if (WarmupFinished)
            {
                ExerciseDetails = "Finished";
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Warmup exercise {WarmupSetIndex + 1}/{WarmupSets.Count()}: ")
                .Append($"{NextWarmupSet.Reps} reps of {WarmupExercise.Name} at a weight of ");

            try
            {
                float weight = NextWarmupSet.GetWeight(WarmupExercise, Single.Parse(WeightInputText));
                builder.Append(weight.ToString());

                if (WarmupExercise.PlateMath != null)
                {
                    builder.Append(" (" + WarmupExercise.PlateMath.PlateCountsToString(weight) + ")");
                }
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

        void RefreshFullWarmupDetails()
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                float weight = Single.Parse(WeightInputText);

                int i = 0;
                foreach (IWarmupSet ws in WarmupSets)
                {
                    if (i == WarmupSetIndex)
                    {
                        builder.Append("> ");
                    }

                    builder.Append("Weight of ");
                    builder.Append(ws.GetWeight(WarmupExercise, weight).ToString());
                    builder.Append(", rest ");
                    builder.Append(ws.GetRestPeriod(WarmupExercise).ToString());

                    if (i < WarmupSetIndex)
                    {
                        builder.Append(" (done)");
                    }

                    if (i < WarmupSets.Length - 1)
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

        void WarmupSetFinished()
        {
            WarmupSetIndex++;

            if (WarmupFinished)
            {
                navigationService.GoBack();
                TimerViewModel.StartTimer(WarmupExercise.RestPeriodSeconds);
                return;
            }


            TimerViewModel.StartTimer(NextWarmupSet.GetRestPeriod(WarmupExercise));

            // TryShowFullScreenAd();
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

        RelayCommand _BackButtonCommand;
        public RelayCommand BackButtonCommand
        {
            get
            {
                return _BackButtonCommand ??
                    (_BackButtonCommand = new RelayCommand(delegate
                    {
                        if(WarmupSetIndex == 0)
                        {
                            navigationService.GoBack();
                            return;
                        }

                        DialogService?.DisplayConfirmationYesNoYesNeverShowAgain(
                             "Are you sure you want to end this warmup session? " +
                                " You will lose all of your progress in this warmup.",
                             "ask_for_end_warmup",
                             delegate
                             {
                                 navigationService.GoBack();
                             });
                    }));
            }
        }
    }
}
