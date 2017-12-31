using Foundation;
using System;
using UIKit;
using System.Linq;
using System.Collections.Generic;

using POLift.Core.Service;
using POLift.Core.Model;
using POLift.iOS.DataSources;

using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;

using Unity;

using POLift.Core.ViewModel;

namespace POLift.iOS.Controllers
{
    public partial class ViewRoutineResultsController : UIViewController
    {
        private ViewRoutineResultsViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.ViewRoutineResults;
            }
        }

        public ViewRoutineResultsController (IntPtr handle) : base (handle)
        {
           
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            RoutineResultsTableView.RowHeight = UITableView.AutomaticDimension;
            RoutineResultsTableView.EstimatedRowHeight = 70f;

            try
            {
                Console.WriteLine("try");
                RefreshRoutineResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        RoutineResultsDataSource rrds;
        void RefreshRoutineResults()
        {
            rrds = new RoutineResultsDataSource(
                   Vm.RoutineResults.ToList());
            rrds.EditClicked += Vm.NavigateEditRoutineResult;
            rrds.DeleteClicked += Vm.DeleteRoutineResult;

            RoutineResultsTableView.Source = rrds;
        }

        class RoutineResultsDataSource : DeleteTableViewSource<IRoutineResult>
        {
            public event Action<IRoutineResult> EditClicked;

            public RoutineResultsDataSource(IList<IRoutineResult> data)
                : base(data)
            {

            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                RoutineResultCell rrc =
                    tableView.DequeueReusableCell("routine_result_cell")
                    as RoutineResultCell;
                IRoutineResult rr = DataFromIndexPath(indexPath);
                rrc.Setup(rr, delegate
                {
                    EditClicked?.Invoke(rr);
                });

                return rrc;
            }
        }
    }
}