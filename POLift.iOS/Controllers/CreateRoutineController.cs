
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Vm.Reset();


            //ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
            //    ExerciseSetsDataSource.ExerciseSetsCellId);
            //ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
            //   ExerciseSetsDataSource.ExerciseSetsDeleteCellId);
             
            


            RoutineNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            AddExerciseLink.SetCommand(Vm.AddExerciseCommand);


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

            Vm.ExerciseSets.CollectionChanged += ExerciseSets_CollectionChanged;

            if (this.View.GestureRecognizers != null)
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
            }

            UILongPressGestureRecognizer long_press = new UILongPressGestureRecognizer(lp =>
            {
                UIGestureRecognizerState state = lp.State;

                CGPoint location = lp.LocationInView(ExerciseSetsTableView);

                NSIndexPath path = ExerciseSetsTableView.IndexPathForRowAtPoint(location);
                if (path != null) last_valid_path = path;

                Console.WriteLine("LONG PRESS DETECTED: " + state + " " + location.ToString() + " " + path?.DebugDescription);

                CGPoint center;
                UITableViewCell cell;

                switch (state)
                {
                    case UIGestureRecognizerState.Began:
                        if (path == null) break;
                        source_index_path = path;

                        cell = ExerciseSetsTableView.CellAt(path);

                        // create snapshot centered at cell
                        snapshot = cell.CustomSnapshotFromView(); //cell.SnapshotView(false);
                        center = cell.Center;
                        snapshot.Center = center;
                        snapshot.Alpha = new nfloat(0.0);
                        ExerciseSetsTableView.AddSubview(snapshot);
                        
                        UIView.Animate(0.25, delegate
                        {
                            // make snapshot bigger
                            center.Y = location.Y;
                            snapshot.Center = center;
                            snapshot.Transform = CGAffineTransform.MakeScale(new nfloat(1.05), new nfloat(1.05));
                            snapshot.Alpha = new nfloat(0.98);

                            // hide cell
                            cell.Alpha = new nfloat(0.0);
                        }, delegate
                        {
                            cell.Hidden = true; // hide cell
                        });
                        break;
                    case UIGestureRecognizerState.Changed:
                        if (path == null) break;
                        if (snapshot == null) break;
                        center = snapshot.Center;
                        center.Y = location.Y;
                        snapshot.Center = center;
                        break;
                    /*case UIGestureRecognizerState.Ended:
                        if (path == null) break;
                        if (snapshot == null) break;
                        center = snapshot.Center;
                        center.Y = location.Y;
                        snapshot.Center = center;

                        if (path != null && !path.IsEqual(source_index_path))
                        {
                            //ExerciseSetsTableVie

                            es_data_source?.MoveRow(ExerciseSetsTableView, source_index_path, path);
                        }

                        // source_index_path = path;
                        break;*/
                    default:
                        //if (path == null) break;
                        if (source_index_path == null) break;
                        if (snapshot == null) break;

                        // remove snapshot view and make old cell visible again
                        cell = ExerciseSetsTableView.CellAt(source_index_path);
                        cell.Hidden = false;
                        cell.Alpha = new nfloat(0.0);

                        // move the rows. 
                        if (last_valid_path != null && !last_valid_path.IsEqual(source_index_path))
                        {
                            es_data_source?.MoveRow(ExerciseSetsTableView, source_index_path, last_valid_path);
                        }

                        UIView.Animate(0.25, delegate
                        {
                            snapshot.Center = cell.Center;
                            snapshot.Transform = CGAffineTransform.MakeIdentity();
                            snapshot.Alpha = new nfloat(0.0);

                            cell.Alpha = new nfloat(1.0);
                        }, delegate
                        {
                            snapshot.RemoveFromSuperview();
                            snapshot = null;

                            
                            source_index_path = null;
                        });
                        break;
                }
            });
            ExerciseSetsTableView.AddGestureRecognizer(long_press);
            //this.View.AddGestureRecognizer(long_press);
        }
        NSIndexPath last_valid_path = null;
        NSIndexPath source_index_path = null;
        UIView snapshot = null;

        UIBarButtonItem done;
        UIBarButtonItem edit;
        ExerciseSetsDataSource es_data_source;

        private void ExerciseSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("ExerciseSets_CollectionChanged");
            RefreshExerciseSetsList();
        }

        void RefreshExerciseSetsList()
        {
            Console.WriteLine("RefreshExerciseSetsList");
            ExerciseSetsTableView.Source = es_data_source =
                new ExerciseSetsDataSource(Vm.ExerciseSets, Vm.LockedExerciseSets);
        }

        class ExerciseSetsDataSource : UITableViewSource
        {
            public static NSString ExerciseSetsCellId = new NSString("ExerciseSetsCell");
            public static NSString ExerciseSetsDeleteCellId = new NSString("ExerciseSetsDeleteCell");

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

                if (cell.GestureRecognizers != null)
                {
                    Console.WriteLine("Cell");
                    foreach (UIGestureRecognizer rec in cell.GestureRecognizers)
                    {
                        Console.WriteLine("UIGestureRecognizer " + rec.DebugDescription);
                        rec.CancelsTouchesInView = false;
                    }
                }

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
        }

    }
}