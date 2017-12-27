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



namespace POLift.iOS.Controllers
{
    public partial class CreateRoutineController : DatabaseController
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
                () => RoutineNameTextField.Text,
                () => Vm.RoutineNameInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            CreateRoutineButton.SetCommand(Vm.CreateRoutineCommand);


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
        }
        UIBarButtonItem done;
        UIBarButtonItem edit;
        ExerciseSetsDataSource es_data_source;


        private void ExerciseSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshExerciseSetsList();
        }

        void RefreshExerciseSetsList()
        {

            ExerciseSetsTableView.Source = es_data_source =
                new ExerciseSetsDataSource(Vm.ExerciseSets);
        }

        class ExerciseSetsDataSource : UITableViewSource
        {
            public static NSString ExerciseSetsCellId = new NSString("ExerciseSetsCell");
            public static NSString ExerciseSetsDeleteCellId = new NSString("ExerciseSetsDeleteCell");

            public ObservableCollection<IExerciseSets> ExerciseSets;

            public ExerciseSetsDataSource(ObservableCollection<IExerciseSets> exercise_sets)
            {
                this.ExerciseSets = exercise_sets;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return ExerciseSets.Count();
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell("exercise_sets_cell")
                     as ExerciseSetsCell;

                cell.Setup(ExerciseSets.ElementAt(indexPath.Row));

                return cell;
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                // return false for locked rows
                
                return true;
            }

            public override void CommitEditingStyle(UITableView tableView,
                UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    ExerciseSets.RemoveAt(indexPath.Row);

                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, 
                        UITableViewRowAnimation.Fade);
                }
            }

            public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return UITableViewCellEditingStyle.Delete;
            }

            public override void MoveRow(UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
            {
                var item = ExerciseSets[sourceIndexPath.Row];
                var deleteAt = sourceIndexPath.Row;
                var insertAt = destinationIndexPath.Row;

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