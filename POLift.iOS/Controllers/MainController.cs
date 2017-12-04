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

            RoutinesTableView.Source = new RoutinesDataSource(
                Helpers.MainPageRoutinesList(Database));
        }
       
        class RoutinesDataSource : UITableViewSource
        {
            public static NSString RoutineCellId = new NSString("RoutineCell");

            List<IRoutine> routines;

            public RoutinesDataSource(IEnumerable<IRoutine> routines)
            {
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
        }
    }
}