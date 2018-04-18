using Foundation;
using POLift.Core.Model;
using System;
using System.Collections.Generic;
using UIKit;

using POLift.Core.ViewModel;

using SidebarNavigation;
using POLift.Core.Service;
using POLift.iOS.Service;

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

        List<List<INavigation>> Navigations;
        List<INavigation> purchase_section;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //NavigationLinkTableView.RegisterClassForCellReuse(
            //  typeof(NavigationCell), NavigationDataSource.NavigationCellId);

            List<INavigation> help_section = new List<INavigation>()
            {
                new Navigation("Settings", Settings_Click, "SettingsIcon"),
                new Navigation("Help & feedback", HelpAndFeedback_Click, "HelpIcon")
            };

            purchase_section = new List<INavigation>();
             // FF545454

            Navigations = new List<List<INavigation>>()
            {
                new List<INavigation>()
                {
                    new Navigation("Select routine", Vm.SelectRoutineNavigate, "SelectRoutineIcon"),
                    new Navigation("View recent sessions", Vm.ViewRecentSessionsNavigate, "CalendarIcon"),
                    new Navigation("View 1RM graphs", Vm.ViewOrmGraphsNavigate, "LineChartIcon")
                },
                new List<INavigation>()
                {
                    new Navigation("Get free lifting programs", Vm.GetFreeWeightliftingProgramsWithNavigationService,
                        "DownloadIcon")
                },
                help_section,
                purchase_section

                
                //    Resource.Mipmap.ic_cloud_download_white_24dp),
                //new Navigation("Backup data", BackupData_Click),
                /*new Navigation("Import data from backup", RestoreData_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),
                new Navigation("Import routines and exercises only", ImportRoutinesAndExercises_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),*/
                //

                
                
                //,

                    /*,
                     * TODO: export data as text
                new Navigation("Export data as text", ExportAsText_Click,
                    Resource.Mipmap.ic_backup_white_24dp)*/
                //new Navigation("On the fly (beta)", Vm.PerformRoutineOnTheFlyNavigate)
            };

            if (Vm.ShowRateApp)
            {
                help_section.Add(new Navigation("Rate app",
                    RateApp_Click, "RateIcon"));
            }

            Vm.ShouldReloadMenu += Vm_ShouldReloadMenu;

            NavigationDataSource nds =
                new NavigationDataSource(Navigations);
            NavigationLinkTableView.SectionHeaderHeight = 20;
            NavigationLinkTableView.Source = nds;
            //NavigationLinkTableView

            nds.RowClicked += Nds_RowClicked;

            AddPurchaseLicenseNavigation();
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        private void Vm_ShouldReloadMenu(object sender, EventArgs e)
        {
            NavigationLinkTableView.ReloadData();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            NSUrl url = new NSUrl(UIApplication.OpenSettingsUrlString);
            UIApplication.SharedApplication.OpenUrl(url);
        }

        async void AddPurchaseLicenseNavigation()
        {
            Tuple<INavigation, INavigation> tup = await Vm.AddPurchaseLicenseNavigation(purchase_section);
            tup.Item1.IconIdentifier = "PurchaseIcon";
            tup.Item2.IconIdentifier = "RestorePurchaseIcon";

            NavigationLinkTableView.ReloadData();
        }

        void RecheckLicense()
        {
            /*StoreKitLicenseManager sklm =
                ViewModelLocator.Default.LicenseManager as StoreKitLicenseManager;

            if (sklm != null)
            {
                System.Diagnostics.Debug.WriteLine("Restoring license...");
                RestoreSklmLicense(sklm);
            }
            else
            {*/
                Vm.RecheckLicense();
            //}
        }

       /* async void RestoreSklmLicense(StoreKitLicenseManager sklm)
        {
            bool result = await sklm.RestoreLicense();

            System.Diagnostics.Debug.WriteLine("Restore License result = " + result);
        }
        */
        private void RateApp_Click(object sender, EventArgs e)
        {
            AppleHelpers.OpenRateApp();
        }

        private void HelpAndFeedback_Click(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(
                new NSUrl(SideMenuViewModel.HelpUrl));
        }

        private void BackupData_Click(object sender, EventArgs e)
        {
            //UIActivityViewController uiavc = new UIActivityViewController()
            NSObject[] obs = new NSObject[] { new NSUrl(AppDelegate.DatabasePath, true) };
            UIActivityViewController uiavc = new UIActivityViewController(obs, null);

            PresentViewController(uiavc, true, null);
           
        }

        private void Nds_RowClicked(INavigation nav)
        {
            nav.OnClick();
            SidebarController.CloseMenu(true);
        }


        public override void ViewDidUnload()
        {
            Vm.ShouldReloadMenu -= Vm_ShouldReloadMenu;

            base.ViewDidUnload();
        }

        class NavigationDataSource : UITableViewSource
        {
            public event Action<INavigation> RowClicked;

            public const string NavigationCellId = "navigation_cell";

            List<List<INavigation>> Navigations;
            public NavigationDataSource(List<List<INavigation>> Navigations)
            {
                this.Navigations = Navigations;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                NavigationCell cell = 
                    tableView.DequeueReusableCell(NavigationCellId) as NavigationCell;

                INavigation nav = IndexPathToNavigation(indexPath);

                UIImage img = nav.IconIdentifier == null ? UIImage.FromBundle("NavigationIcon") : 
                    UIImage.FromBundle(nav.IconIdentifier);

                cell.Setup(img, nav.Text);

                return cell;
            }

            INavigation IndexPathToNavigation(NSIndexPath path)
            {
                return Navigations[path.Section][path.Row];
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                //return section == 0 ? "" : " ";
                return " ";
            }

            public override nint RowsInSection(UITableView tableView, nint section)
            {
                return Navigations[(int)section].Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return Navigations.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                RowClicked?.Invoke(IndexPathToNavigation(indexPath));
            }
        }
    }
}