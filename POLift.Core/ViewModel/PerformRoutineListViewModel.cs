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

    public class PerformRoutineListViewModel : PerformRoutineViewModel
    {
        public PerformRoutineListViewModel(INavigationService navigationService, IPOLDatabase database) :
            base(navigationService, database)
        {
            
        }


    }
}
