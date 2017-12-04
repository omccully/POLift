using Foundation;
using System;
using UIKit;
using POLift.Core.Model;
using System.Collections.Generic;

namespace POLift.iOS.Controllers
{
    public partial class CreateRoutineController : DatabaseController
    {
        ExerciseSetsDataSource es_data_source;

        public CreateRoutineController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExerciseSetsDataSource.ExerciseSetsCellId);

            es_data_source = new ExerciseSetsDataSource(new List<IExerciseSets>());
            ExerciseSetsTableView.Source = es_data_source;
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var ChildController = segue.DestinationViewController as IValueReturner<IExercise>;

            if(ChildController != null)
            {
                ChildController.ValueChosen += ChildController_ExerciseChosen;
            }
        }

        private void ChildController_ExerciseChosen(IExercise exercise)
        {
            // add exercise to ExerciseSets list

            ExerciseSets es = new ExerciseSets(exercise);
            es.Database = Database;
            es_data_source.ExerciseSets.Add(es);
        }

        class ExerciseSetsDataSource : UITableViewSource
        {
            public static NSString ExerciseSetsCellId = new NSString("ExerciseSetsCell");

            public List<IExerciseSets> ExerciseSets;

            public ExerciseSetsDataSource(IEnumerable<IExerciseSets> exercise_sets)
            {
                this.ExerciseSets = new List<IExerciseSets>(exercise_sets);
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return ExerciseSets.Count;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(ExerciseSetsCellId);

                cell.TextLabel.Text = ExerciseSets[indexPath.Row].ToString();

                return cell;
            }
        }
    }
}