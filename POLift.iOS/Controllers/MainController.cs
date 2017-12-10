using System;
using System.Collections.Generic;
using Foundation;
using POLift.Core.Model;
using POLift.Core;
using POLift.Core.Helpers;
using POLift.Core.Service;
using UIKit;

using Unity;
using GalaSoft.MvvmLight.Helpers;
using POLift.Core.ViewModel;

namespace POLift.iOS.Controllers
{
    public partial class MainController : DatabaseController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private MainViewModel Vm
        {
            get
            {
                return Application.Locator.Main;
            }
        }

        public MainController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            this.Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutinesTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                RoutinesDataSource.RoutineCellId);

            RefreshRoutinesList();

            CreateNewRoutineLink.TouchUpInside += (s, e) => { };
            CreateNewRoutineLink.SetCommand(
                "TouchUpInside",
                Vm.CreateRoutineNavigateCommand);
        }

        RoutinesDataSource routine_data_source;
        void RefreshRoutinesList()
        {
            routine_data_source = new RoutinesDataSource(this,
                Vm.RoutinesList);

            routine_data_source.RoutineSelected += Routine_data_source_RoutineSelected;

            RoutinesTableView.Source = routine_data_source;
        }

        private void Routine_data_source_RoutineSelected(object sender, IRoutineWithLatestResult e)
        {
            Console.WriteLine("navigating...");
            Vm.SelectRoutineNavigateCommand(e).Execute(e.Routine);
        }

        /*public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var ChildController = segue.DestinationViewController as IValueReturner<IRoutine>;

            if (ChildController != null)
            {
                ChildController.ValueChosen += ChildController_RoutineCreated;
            }
        }*/

        private void ChildController_RoutineCreated(IRoutine routine)
        {
            RefreshRoutinesList();
        }


        class RoutinesDataSource : UITableViewSource
        {
            public event EventHandler<IRoutineWithLatestResult> RoutineSelected;

            public static NSString RoutineCellId = new NSString("RoutineCell");

            MainController parent;
            List<IRoutineWithLatestResult> data;

            public RoutinesDataSource(MainController parent, 
                IEnumerable<IRoutineWithLatestResult> data)
            {
                this.parent = parent;
                this.data = new List<IRoutineWithLatestResult>(data);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return data.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(RoutineCellId);

                cell.TextLabel.Text = data[indexPath.Row].Routine.ToString();

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                RoutineSelected?.Invoke(this, data[indexPath.Row]);


                /*PerformRoutineController pr = 
                    parent.Storyboard.InstantiateViewController("PerformRoutineController") 
                    as PerformRoutineController;

                if(pr != null)
                {
                    pr.Database = parent.Database;
                    pr.Routine = data[indexPath.Row].Routine;
                    parent.NavigationController.PushViewController(pr, true);
                }*/
            }
        }
    }
}