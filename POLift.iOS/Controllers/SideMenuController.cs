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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationLinkTableView.RegisterClassForCellReuse(
                typeof(UITableViewCell), NavigationDataSource.NavigationCellId);

            List<INavigation> Navigations = new List<INavigation>()
            {
                new Navigation("Select routine", Vm.SelectRoutineNavigate),
                new Navigation("View recent sessions", Vm.ViewRecentSessionsNavigate),
                new Navigation("View 1RM graphs", Vm.ViewOrmGraphsNavigate),

                new Navigation("Get free lifting programs", Vm.GetFreeWeightliftingPrograms),
                //    Resource.Mipmap.ic_cloud_download_white_24dp),
                /*new Navigation("Backup data", BackupData_Click,
                    Resource.Mipmap.ic_backup_white_24dp),
                new Navigation("Import data from backup", RestoreData_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),
                new Navigation("Import routines and exercises only", ImportRoutinesAndExercises_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),*/
                //

                /*new Navigation("Settings", Settings_Click,
                    Resource.Mipmap.ic_settings_white_24dp),
                new Navigation("Help & feedback", HelpAndFeedback_Click,
                    Resource.Mipmap.ic_help_white_24dp)*/

                    /*,
                     * TODO: export data as text
                new Navigation("Export data as text", ExportAsText_Click,
                    Resource.Mipmap.ic_backup_white_24dp)*/
            };

            NavigationDataSource nds = 
                new NavigationDataSource(Navigations);

            NavigationLinkTableView.Source = nds;

            nds.RowClicked += Nds_RowClicked;
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

                cell.TextLabel.Text = Navigations[indexPath.Row].Text;

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