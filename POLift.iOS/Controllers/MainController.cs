using System;
using System.Collections.Generic;
using Foundation;
using POLift.Core.Model;
using POLift.Core;
using POLift.Core.Helpers;
using POLift.Core.Service;
using System.Linq;
using UIKit;

using Unity;
using GalaSoft.MvvmLight.Helpers;
using POLift.Core.ViewModel;
using POLift.iOS.DataSources;
using System.Threading.Tasks;

using SidebarNavigation;
using POLift.iOS.Service;

namespace POLift.iOS.Controllers
{
    public partial class MainController : UIViewController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private MainViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.Main;
            }
        }

        public MainController(IntPtr intptr) : base(intptr)
        {

        }

        public MainController() 
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            RoutinesTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                RoutinesDataSource.GetCellId<IRoutineWithLatestResult>());

            //RefreshRoutinesList();

            /*CreateNewRoutineLink.TouchUpInside += (s, e) => {
                this.NavigationController.PushViewController(
                    this.Storyboard.InstantiateViewController(
                    "CreateRoutinePage"), true);
            };*/

            Vm.RoutineCompleted += Vm_RoutineCompleted;

            CreateNewRoutineLink.SetCommand(
                "TouchUpInside",
                Vm.CreateRoutineNavigateCommand);

            LiftOnTheFlyButton.SetCommand(
                "TouchUpInside",
                Vm.PerformRoutineOnTheFlyNavigateCommand);

            RoutinesTableView.RowHeight = UITableView.AutomaticDimension;
            RoutinesTableView.EstimatedRowHeight = 40f;

            Vm.RoutinesListChanged += Vm_RoutinesListChanged;
            Console.WriteLine("load");
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();

            Vm.RoutineCompleted -= Vm_RoutineCompleted;
        }

        private void Vm_RoutineCompleted(IRoutineResult obj)
        {
            // TODO: ask for backup

            Action share_rr_action = delegate
            {
                Vm.AskForShareRoutineResult(obj, delegate
                {
                    this.ShareRoutineResult(obj);
                });
            };

            if(Vm.TotalRoutineResults >= 15)
            {
                // executes share_rr_action if dialog doesn't appear as well
                Vm.DialogService.DisplayAcknowledgementOnce(
                    "It's highly recommended that you ensure that iCloud backups " +
                    "are enabled for the POLift app. This will make sure that you " + 
                    "can regain access to your POLift data if your phone gets lost, stolen, or broken. " +
                    "This can be done in your iCloud settings.",
                    SideMenuViewModel.AskForBackupPreferenceKey,
                    share_rr_action);
            }
            else
            {
                share_rr_action();
            } 
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            RefreshRoutinesList(true);
            Console.WriteLine("appear");
        }

        private void Vm_RoutinesListChanged(object sender, EventArgs e)
        {
            RefreshRoutinesList(false);
        }

        RoutinesDataSource routine_data_source;
        void RefreshRoutinesList(bool refresh_source = false)
        {
            if (refresh_source)
            {
                Vm.RefreshRoutinesList(false);
            }

            routine_data_source = new RoutinesDataSource(Vm.RoutinesList.ToList());

            routine_data_source.RowClicked += Routine_data_source_RoutineSelected;
            routine_data_source.DeleteClicked += Vm.DeleteRoutine;
            routine_data_source.EditClicked += Vm.EditRoutineNavigation;

            RoutinesTableView.Source = routine_data_source;
            RoutinesTableView.ReloadData();
        }


        private void Routine_data_source_RoutineSelected(object sender, IRoutineWithLatestResult e)
        {
            Console.WriteLine("navigating...");
            Vm.SelectRoutineNavigateCommand(e).Execute(e.Routine);
        }

        class RoutinesDataSource : DeleteTableViewSource<IRoutineWithLatestResult>
        {
            public event Action<IRoutine> EditClicked;

            public RoutinesDataSource(IList<IRoutineWithLatestResult> Data) : 
                base(Data)
            {

            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                RoutineCell cell = tableView.DequeueReusableCell("routine_cell")
                    as RoutineCell;

                IRoutineWithLatestResult routineinfo = DataFromIndexPath(indexPath);

                cell.Setup(routineinfo, delegate
                {
                    EditClicked?.Invoke(routineinfo.Routine);
                });

                return cell;
            }

            protected override string GetTextLabelText(NSIndexPath indexPath)
            {
                return DataFromIndexPath(indexPath).Routine.ToString();
            }
        }
    }
}