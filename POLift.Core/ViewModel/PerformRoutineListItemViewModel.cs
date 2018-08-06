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
    public class PerformRoutineListItemViewModel : ViewModelBase
    {
        public string ExerciseName { get; set; }

        public string WeightInput { get; set; }
        public string RepInput { get; set; }

        public bool WeightInputEnabled { get; set; }
        public bool RepInputEnabled { get; set; }

    }
}
