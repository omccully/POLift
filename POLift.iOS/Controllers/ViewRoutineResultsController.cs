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

using Social;
using POLift.iOS.Service;
using POLift.iOS;

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

            //RoutineResultsTableView.AllowsSelection = false;

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
            rrds.ShareClicked += Rrds_ShareClicked;
            RoutineResultsTableView.Source = rrds;

            RoutineResultsTableView.RowHeight = UITableView.AutomaticDimension;
            RoutineResultsTableView.EstimatedRowHeight = 120f;
            RoutineResultsTableView.ReloadData();
        }

        private void Rrds_ShareClicked(IRoutineResult obj)
        {
            /*TwitterService ts = new TwitterService();
            ts.ConsumerKey = "Z7V4YV2Bw2Z1FZ7QC5Id9uQbn";
            ts.ConsumerSecret = "15uNGW3vjBSt1xIBnSf3gmkaZJJs8kMrFiBPsK6nP8xDYbYces";
            ts.CallbackUrl = new Uri("http://polift-app.com");
            
            Item item = new Item(obj.ToString());

            var share_cont = ts.GetShareUI(item, result =>
            {
                DismissViewController(true, null);
            });
            PresentViewController(share_cont, true, null);*/
           
            this.ShareRoutineResult(obj);
        }

        class RoutineResultsDataSource : DeleteTableViewSource<IRoutineResult>
        {
            public event Action<IRoutineResult> EditClicked;
            public event Action<IRoutineResult> ShareClicked;

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
                },
                delegate
                {
                    ShareClicked?.Invoke(rr);
                });

                return rrc;
            }
        }
    }
}