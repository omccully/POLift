using Foundation;
using System;
using UIKit;
using POLift.Core.Model;
using POLift.Core.Service;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using POLift.Core.ViewModel;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Helpers;
using CoreGraphics;
using POLift.iOS.Service;

namespace POLift.iOS.Controllers
{
    public partial class CreateRoutineController : UIViewController
    {
        private readonly List<Binding> bindings = new List<Binding>();

        private CreateRoutineViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.CreateRoutine;
            }
        }

        public CreateRoutineController(IntPtr handle) : base(handle)
        {
        }

        public string SubmitButtonText
        {
            get
            {
                return CreateRoutineButton.Title(UIControlState.Normal);
            }
            set
            {
                CreateRoutineButton.SetTitle(value, UIControlState.Normal);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Vm.Reset();


            //ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
            //    ExerciseSetsDataSource.ExerciseSetsCellId);
            //ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
            //   ExerciseSetsDataSource.ExerciseSetsDeleteCellId);

            RoutineNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            //TipLabel.Text = "Tip: Long press to move, swipe left to delete\n" +
             //   "Tap to edit";

            AddExerciseLink.SetCommand(Vm.AddExerciseCommand);

            bindings.Add(this.SetBinding(
                () => Vm.Title,
                () => Title));

            bindings.Add(this.SetBinding(
                () => Vm.SubmitButtonText,
                () => SubmitButtonText));

            bindings.Add(this.SetBinding(
                () => Vm.RoutineNameInput,
                () => RoutineNameTextField.Text, 
                BindingMode.TwoWay)
                .ObserveTargetEvent("EditingChanged"));

            CreateRoutineButton.SetCommand(Vm.CreateRoutineCommand);

            ExerciseSetsTableView.EstimatedRowHeight = 57;
            ExerciseSetsTableView.RowHeight = 57;

            RefreshExerciseSetsList();

            done = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                ExerciseSetsTableView.SetEditing(false, true);
                NavigationItem.RightBarButtonItem = edit;
                
            });

            edit = new UIBarButtonItem(UIBarButtonSystemItem.Edit, delegate
            {
                ExerciseSetsTableView.SetEditing(true, true);
                NavigationItem.RightBarButtonItem = done;
            });

            NavigationItem.RightBarButtonItem = edit;

            ExerciseSetsTableView.SetEditing(false, true);

            //Vm.ExerciseSets.CollectionChanged += ExerciseSets_CollectionChanged;
            Vm.ExerciseSetsChanged += Vm_ExerciseSetsChanged;

           /* if (this.View.GestureRecognizers != null)
            {
                Console.WriteLine("CreateRoutineController");
                foreach (UIGestureRecognizer rec in this.View.GestureRecognizers)
                {
                    Console.WriteLine("UIGestureRecognizer " + rec.DebugDescription);
                    rec.CancelsTouchesInView = false;
                }
            }

            foreach (UIGestureRecognizer rec in ExerciseSetsTableView.GestureRecognizers)
            {
                if(rec.Class.Name == "UIScrollViewDelayedTouchesBeganGestureRecognizer")
                {
                    rec.Enabled = false;
                }
                Console.WriteLine(ExerciseSetsTableView.GestureRecognizers);
                Console.WriteLine("UIGestureRecognizer " + rec.DebugDescription);
                Console.WriteLine("UIGestureRecognizer " + rec.GetType());
                Console.WriteLine("UIGestureRecognizer " + rec.Class.Name);

                rec.CancelsTouchesInView = false;
            }*/

            cell_mover =
                new LongPressTableViewCellMover(ExerciseSetsTableView);
            cell_mover.GestureRecognizer.CancelsTouchesInView = false;
            ExerciseSetsTableView.AddGestureRecognizer(cell_mover.GestureRecognizer);
        }

        private void Vm_ExerciseSetsChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Vm_ExerciseSetsChanged");
            RefreshExerciseSetsList();
        }

        LongPressTableViewCellMover cell_mover;

        UIBarButtonItem done;
        UIBarButtonItem edit;
        ExerciseSetsDataSource es_data_source;

        /*private void ExerciseSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("ExerciseSets_CollectionChanged");
            RefreshExerciseSetsList();
        }*/

        void RefreshExerciseSetsList()
        {
            Console.WriteLine("RefreshExerciseSetsList");
            ExerciseSetsTableView.Source = es_data_source =
                new ExerciseSetsDataSource(Vm.ExerciseSets, Vm.LockedExerciseSets);
            es_data_source.SelectedExerciseSets += Es_data_source_SelectedExerciseSets;
        }

        private void Es_data_source_SelectedExerciseSets(int index, IExerciseSets obj)
        {
            // edit Exercise

            Vm.EditExerciseAtIndex(index);
        }

        class ExerciseSetsDataSource : UITableViewSource
        {
            public static NSString ExerciseSetsCellId = new NSString("ExerciseSetsCell");
            public static NSString ExerciseSetsDeleteCellId = new NSString("ExerciseSetsDeleteCell");

            public event Action<int, IExerciseSets> SelectedExerciseSets;

            public ObservableCollection<IExerciseSets> ExerciseSets;
            int LockedExerciseSets;

            public ExerciseSetsDataSource(ObservableCollection<IExerciseSets> exercise_sets,
                int locked_exercise_sets = 0)
            {
                this.ExerciseSets = exercise_sets;
                this.LockedExerciseSets = locked_exercise_sets;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return ExerciseSets.Count();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("exercise_sets_cell")
                     as ExerciseSetsCell;

                cell.Setup(ExerciseSets.ElementAt(indexPath.Row), 
                    CanEditRow(tableView, indexPath));

                /*if (cell.GestureRecognizers != null)
                {
                    Console.WriteLine("Cell");
                    foreach (UIGestureRecognizer rec in cell.GestureRecognizers)
                    {
                        Console.WriteLine("UIGestureRecognizer " + rec.DebugDescription);
                        rec.CancelsTouchesInView = false;
                    }
                }*/

                return cell;
            }

            bool CanEditRow(int row)
            {
                return (row >= LockedExerciseSets);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                // return false for locked rows
                return CanEditRow(indexPath.Row);
            }

            public override void CommitEditingStyle(UITableView tableView,
                UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                System.Diagnostics.Debug.WriteLine($"CommitEditingStyle " + indexPath.Row);
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    //tableView.DeleteRows(new NSIndexPath[] { indexPath },
                      //  UITableViewRowAnimation.Fade);

                    ExerciseSets.RemoveAt(indexPath.Row);

                    
                }
            }

            public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
            {
                return CanEditRow(tableView, indexPath);
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.Delete;
            }

            public override NSIndexPath CustomizeMoveTarget(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath proposedIndexPath)
            {
                if(CanEditRow(proposedIndexPath.Row))
                {
                    return proposedIndexPath;
                }
                return sourceIndexPath;
            }

            public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
            { 
                var item = ExerciseSets[sourceIndexPath.Row];
                int deleteAt = sourceIndexPath.Row;
                int insertAt = destinationIndexPath.Row;
                System.Diagnostics.Debug.WriteLine($"try [{deleteAt}] -> [{insertAt}]");

                if (deleteAt == insertAt) return;

                // are we inserting 
                if (destinationIndexPath.Row < sourceIndexPath.Row)
                {
                    // add one to where we delete, because we're increasing the index by inserting
                    deleteAt += 1;
                }
                else
                {
                    // add one to where we insert, because we haven't deleted the original yet
                    insertAt += 1;
                }

                ExerciseSets.Insert(insertAt, item);
                ExerciseSets.RemoveAt(deleteAt);
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                System.Diagnostics.Debug.WriteLine("RowSelected " + indexPath.DebugDescription);

                SelectedExerciseSets?.Invoke(indexPath.Row, ExerciseSets.ElementAt(indexPath.Row));
            }
        }

    }
}