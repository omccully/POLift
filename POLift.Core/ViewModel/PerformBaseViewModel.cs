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

    public abstract class PerformBaseViewModel : ViewModelBase
    {
        protected readonly INavigationService navigationService;
        protected readonly IPOLDatabase Database;

        public IDialogService DialogService;
        public ITimerViewModel TimerViewModel;

        

        public PerformBaseViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
        }

        public event EventHandler ResultSubmittedWithoutCompleting;
        protected void OnResultSubmittedWithoutCompleting(EventArgs args = null)
        {
            ResultSubmittedWithoutCompleting?.Invoke(this, 
                args == null ? new EventArgs() : args);
        }

        public virtual IExercise CurrentExercise { get; set; }

        public IPlateMath CurrentPlateMath
        {
            get
            {
                if (CurrentExercise == null) return null;
                return PlateMath.PlateMathTypes[CurrentExercise.PlateMathID];
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

        string _WeightInputText;
        public string WeightInputText
        {
            get
            {
                return _WeightInputText;
            }
            set
            {
                SetPlateMath(value);

                Set(() => WeightInputText, ref _WeightInputText, value); 
            }
        }

        protected void SetPlateMath(string weight_input)
        {
            try
            {
                SetPlateMath(Single.Parse(weight_input));
            }
            catch
            {
                PlateMathDetails = "";
            }
        }
        
        protected virtual void SetPlateMath(float weight_input)
        {
            if (CurrentPlateMath == null)
            {
                PlateMathDetails = "";
            }
            else
            {
                string plate_counts_str = CurrentPlateMath.PlateCountsToString(weight_input);
                PlateMathDetails = $" ({plate_counts_str})";
            }
        }

        protected void RefreshDetails()
        {
            RefreshRoutineDetails();
            RefreshExerciseDetails();
        }

        protected abstract void RefreshExerciseDetails();

        public abstract void RefreshRoutineDetails();

       
        public const string WorkingSetWeightKey = "working_set_weight";
        public virtual void SaveState(KeyValueStorage kvs)
        {
            kvs.SetValue(WorkingSetWeightKey, WeightInputText);
        }

        public virtual void RestoreState(KeyValueStorage kvs)
        {
            string wit = kvs.GetString(WorkingSetWeightKey, null);
            if (wit != null) WeightInputText = wit;
        }
    }
}
