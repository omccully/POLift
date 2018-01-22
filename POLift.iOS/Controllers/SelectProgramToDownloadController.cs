using Foundation;
using POLift.Core.Model;
using System;
using UIKit;
using System.Linq;
using Unity;
using Microsoft.Practices.ServiceLocation;
using POLift.Core.Service;
using POLift.Core.ViewModel;
using System.Collections.Generic;

namespace POLift.iOS.Controllers
{
    public partial class SelectProgramToDownloadController : UIViewController
    {
        private SelectProgramToDownloadViewModel Vm
        {
            get
            {
                return 
                    ViewModelLocator.Default.SelectProgramToDownload;
            }
        }

        ExternalProgram[] Programs = new ExternalProgram[0];
        IPOLDatabase Database;

        public SelectProgramToDownloadController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Database = ServiceLocator.Current.GetInstance<IPOLDatabase>();

            ProgramsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ProgramsTableViewSource.CellId);
            
            Vm.ProgramsChanged += Vm_ProgramsChanged;

            Vm.RefreshProgramsList();
        }

        ProgramsTableViewSource ptvs;
        private void Vm_ProgramsChanged(object sender, EventArgs e)
        {
            //Vm.Programs
            ptvs = new ProgramsTableViewSource(Vm.Programs);
            ptvs.RowClicked += Vm.SelectExternalProgram;
            ProgramsTableView.Source = ptvs;
            ProgramsTableView.ReloadData();
        }

        class ProgramsTableViewSource : UITableViewSource
        {
            public const string CellId = "external_programs_cell";

            public event Action<ExternalProgram> RowClicked;

            IEnumerable<ExternalProgram> programs;
            public ProgramsTableViewSource(IEnumerable<ExternalProgram> programs)
            {
                this.programs = programs;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell(CellId);

                cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
                cell.TextLabel.Lines = 2;
                cell.TextLabel.Text = programs
                    .ElementAt((int)indexPath.Row)
                    .title;

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return programs.Count();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                RowClicked?.Invoke(programs
                    .ElementAt((int)indexPath.Row));
            }
        }
    }
}