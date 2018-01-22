using Foundation;
using POLift.Core.Model;
using System;
using System.Collections.Generic;
using UIKit;

using POLift.Core.ViewModel;

using SidebarNavigation;

namespace POLift.iOS.Controllers
{
    public partial class SideMenuController : UIViewController
    {
        public SidebarController SidebarController;

        private SideMenuViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SideMenu;
            }
        }

        public SideMenuController (IntPtr handle) : base (handle)
        {
        }
        List<INavigation> Navigations;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationLinkTableView.RegisterClassForCellReuse(
                typeof(UITableViewCell), NavigationDataSource.NavigationCellId);

            Navigations = new List<INavigation>()
            {
                new Navigation("Select routine", Vm.SelectRoutineNavigate),
                new Navigation("View recent sessions", Vm.ViewRecentSessionsNavigate),
                new Navigation("View 1RM graphs", Vm.ViewOrmGraphsNavigate),

                new Navigation("Get free lifting programs", Vm.GetFreeWeightliftingPrograms),
                //    Resource.Mipmap.ic_cloud_download_white_24dp),
                //new Navigation("Backup data", BackupData_Click),
                /*new Navigation("Import data from backup", RestoreData_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),
                new Navigation("Import routines and exercises only", ImportRoutinesAndExercises_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),*/
                //

                new Navigation("Settings", Settings_Click),
                new Navigation("Help & feedback", HelpAndFeedback_Click)

                    /*,
                     * TODO: export data as text
                new Navigation("Export data as text", ExportAsText_Click,
                    Resource.Mipmap.ic_backup_white_24dp)*/
            };

            if(Vm.ShowRateApp)
            {
                Navigations.Add(new Navigation("Rate app", RateApp_Click));
            }

            NavigationDataSource nds = 
                new NavigationDataSource(Navigations);

            NavigationLinkTableView.Source = nds;

            nds.RowClicked += Nds_RowClicked;

            AddPurchaseLicenseNavigation();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NSUrl url = new NSUrl(UIApplication.OpenSettingsUrlString);
            UIApplication.SharedApplication.OpenUrl(url);
        }

        async void AddPurchaseLicenseNavigation()
        {
            Navigation purchase_license_nav =
                await Vm.GetPurchaseLicenseNavigationLink();
            if (purchase_license_nav != null)
            {
                Navigations.Add(purchase_license_nav);
                NavigationLinkTableView.ReloadData();
            }
        }

        private void RateApp_Click(object sender, EventArgs e)
        {
           
        }

        private void HelpAndFeedback_Click(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(
                new NSUrl(SideMenuViewModel.HelpUrl));
        }

        private void BackupData_Click(object sender, EventArgs e)
        {
            //UIActivityViewController uiavc = new UIActivityViewController()
            
        }

        private void Nds_RowClicked(INavigation nav)
        {
            nav.OnClick();
            SidebarController.CloseMenu(true);
        }

        class NavigationDataSource : UITableViewSource
        {
            public event Action<INavigation> RowClicked;

            public const string NavigationCellId = "navigation_cell";

            List<INavigation> Navigations;
            public NavigationDataSource(List<INavigation> Navigations)
            {
                this.Navigations = Navigations;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = 
                    tableView.DequeueReusableCell(NavigationCellId);

                cell.BackgroundColor = UIColor.Clear;
                cell.TextLabel.Text = Navigations[indexPath.Row].Text;
                cell.TextLabel.TextColor = UIColor.White;
                return cell;
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return Navigations.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                //.OnClick();
                RowClicked?.Invoke(Navigations[indexPath.Row]);
            }
        }
    }
}