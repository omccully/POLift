﻿using System;
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
                return ViewModelLocator.Default.Main;
            }
        }

        public MainController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            RoutinesTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                RoutinesDataSource.GetCellId<IRoutineWithLatestResult>());

            RefreshRoutinesList();

            CreateNewRoutineLink.TouchUpInside += (s, e) => { };
            CreateNewRoutineLink.SetCommand(
                "TouchUpInside",
                Vm.CreateRoutineNavigateCommand);

            Vm.RoutinesListChanged += Vm_RoutinesListChanged;
        }

        private void Vm_RoutinesListChanged(object sender, EventArgs e)
        {
            RefreshRoutinesList();
        }

        RoutinesDataSource routine_data_source;
        void RefreshRoutinesList()
        {
            routine_data_source = new RoutinesDataSource(Vm.RoutinesList.ToList());

            routine_data_source.RowClicked += Routine_data_source_RoutineSelected;
            routine_data_source.DeleteClicked += Vm.DeleteRoutine;

            RoutinesTableView.Source = routine_data_source;
            RoutinesTableView.Delegate = new RoutinesTableViewDelegate();
        }

        private void Routine_data_source_RoutineSelected(object sender, IRoutineWithLatestResult e)
        {
            Console.WriteLine("navigating...");
            Vm.SelectRoutineNavigateCommand(e).Execute(e.Routine);
        }

        class RoutinesTableViewDelegate : UITableViewDelegate
        {
            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                Console.WriteLine("RoutinesTableViewDelegate.RowSelected");
            }
        }

        class RoutinesDataSource : DeleteTableViewSource<IRoutineWithLatestResult>
        {
            public RoutinesDataSource(IList<IRoutineWithLatestResult> Data) : 
                base(Data)
            {

            }

            protected override string GetTextLabelText(NSIndexPath indexPath)
            {
                return Data[indexPath.Row].Routine.ToString();
            }
        }
    }
}