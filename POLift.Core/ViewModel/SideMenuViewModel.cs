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
    using Model;
    using Service;

    public class SideMenuViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase database;

        public SideMenuViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.database = database;
        }

        public void SelectRoutineNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.MainPageKey);
        }

        public void ViewRecentSessionsNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.ViewRoutineResultsPageKey);
        }

        public void ViewOrmGraphsNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.OrmGraphPageKey);
        }

        public void GetFreeWeightliftingPrograms()
        {
            navigationService.NavigateTo(
                ViewModelLocator.SelectProgramToDownloadPageKey);
        }
    }
}
