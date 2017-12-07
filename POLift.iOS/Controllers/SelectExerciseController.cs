using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

using POLift.Core.Service;
using POLift.Core.Model;

namespace POLift.iOS.Controllers
{
    public partial class SelectExerciseController : DatabaseController, IValueReturner<IExercise>
    {
        public event Action<IExercise> ValueChosen;

        public SelectExerciseController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExerciseListTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExercisesDataSource.ExerciseCellId);

            ExerciseListTableView.Source = new ExercisesDataSource(this, 
                Database.Table<Exercise>());

            //ExerciseListTableView.RowSelected
            //ExerciseListTableView.IndexPathForSelectedRow
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var ChildController = segue.DestinationViewController as IValueReturner<IExercise>;

            if (ChildController != null)
            {
                ChildController.ValueChosen += ReturnExercise;
            }
        }


        void ReturnExercise(IExercise exercise)
        {
            // pass it up to parent
            ValueChosen?.Invoke(exercise);

            NavigationController.PopViewController(true);
        }

        class ExercisesDataSource : UITableViewSource
        {
            public static NSString ExerciseCellId = new NSString("ExerciseCell");

            SelectExerciseController parent;
            List<Exercise> exercises;

            public ExercisesDataSource(SelectExerciseController parent, 
                IEnumerable<Exercise> exercises)
            {
                this.parent = parent;
                this.exercises = new List<Exercise>(exercises);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(ExerciseCellId);

                cell.TextLabel.Text = exercises[indexPath.Row].ToString();

                return cell;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return exercises.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                Console.WriteLine($"calling parent.ReturnExercise(exercises[{indexPath.Row}])");
                parent.ReturnExercise(exercises[indexPath.Row]);
            }
        }
    }
}