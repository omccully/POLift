using System;
using System.Collections.Generic;
using Foundation;
using POLift.Core.Model;
using POLift.Core;
using POLift.Core.Helpers;
using POLift.Core.Service;
using UIKit;

using Unity;

namespace POLift.iOS.Controllers
{
    public partial class MainController : DatabaseController
    {
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
        }

        void RefreshRoutinesList()
        {
            RoutinesTableView.Source = new RoutinesDataSource(this, 
                Helpers.MainPageRoutinesList(Database));
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var ChildController = segue.DestinationViewController as IValueReturner<IRoutine>;

            if (ChildController != null)
            {
                ChildController.ValueChosen += ChildController_RoutineCreated;
            }
        }

        private void ChildController_RoutineCreated(IRoutine routine)
        {
            RefreshRoutinesList();
        }

        class RoutinesDataSource : UITableViewSource
        {
            public static NSString RoutineCellId = new NSString("RoutineCell");

            MainController parent;
            List<IRoutine> routines;

            public RoutinesDataSource(MainController parent, IEnumerable<IRoutine> routines)
            {
                this.parent = parent;
                this.routines = new List<IRoutine>(routines);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return routines.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(RoutineCellId);

                cell.TextLabel.Text = routines[indexPath.Row].ToString();

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                PerformRoutineController pr = 
                    parent.Storyboard.InstantiateViewController("PerformRoutineController") 
                    as PerformRoutineController;

                if(pr != null)
                {
                    pr.Database = parent.Database;
                    pr.Routine = routines[indexPath.Row];
                    parent.NavigationController.PushViewController(pr, true);
                }
            }
        }
    }
}