using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POLift.Core.Model;
using POLift.Core.Service;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

namespace POLift.Core.ViewModel
{
    public class EditRoutineResultViewModel : ViewModelBase, IEditRoutineResultViewModel
    {
        public event Action DoneEditing;

        private readonly INavigationService navigationService;

        public EditRoutineResultViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public void Reset()
        {
            _RoutineResult = null;
            TimeDetailsText = null;
            WeightEdits = new Dictionary<int, float>();
            RepsEdits = new Dictionary<int, int>();
        }

        IRoutineResult _RoutineResult;
        public IRoutineResult RoutineResult
        {
            get
            {
                return _RoutineResult;
            }
            set
            {
                Reset();
                _RoutineResult = value;

                TimeDetailsText = value.TimeDetails;
            }
        }

        public string TimeDetailsText { get; set; }

        public Dictionary<int, float> WeightEdits = new Dictionary<int, float>();
        public Dictionary<int, int> RepsEdits = new Dictionary<int, int>();

        public void SaveEdits()
        {
            RoutineResult.SaveEdits(WeightEdits, RepsEdits);
        }

        public void Done()
        {
            SaveEdits();
            navigationService.GoBack();
            DoneEditing?.Invoke();
        }

        RelayCommand _DoneCommand;
        public RelayCommand DoneCommand
        {
            get
            {
                return _DoneCommand ??
                    (_DoneCommand = new RelayCommand(Done));
            }
        }
    }
}
