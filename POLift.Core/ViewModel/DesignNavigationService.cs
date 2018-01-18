using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    class DesignNavigationService : INavigationService
    {
        public string CurrentPageKey {
            get {
                return "none";
            }
        }

        public void GoBack()
        {
            //throw new NotImplementedException();
        }

        public void NavigateTo(string pageKey)
        {
            //throw new NotImplementedException();
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            //throw new NotImplementedException();
        }
    }
}
