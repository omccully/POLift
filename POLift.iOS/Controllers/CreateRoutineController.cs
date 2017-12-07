using Foundation;
using System;
using UIKit;
using POLift.Core.Model;
using POLift.Core.Service;
using System.Collections.Generic;

namespace POLift.iOS.Controllers
{
    public partial class CreateRoutineController : DatabaseController, IValueReturner<IRoutine>
    {
        ExerciseSetsDataSource es_data_source;

        public event Action<IRoutine> ValueChosen;

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

            RoutineNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            CreateRoutineButton.TouchUpInside += CreateRoutineButton_TouchUpInside;
        }

        List<IExerciseSets> SaveExerciseSets()
        {
            List<IExerciseSets> normalized = es_data_source.ExerciseSets.NormalizeExerciseSets(Database);
            foreach (IExerciseSets ex_sets in normalized)
            {
                Database.InsertOrUpdateNoID<ExerciseSets>((ExerciseSets)ex_sets);
            }

            return normalized;
        }

        private void CreateRoutineButton_TouchUpInside(object sender, EventArgs e)
        {
            if(es_data_source.ExerciseSets.Count == 0)
            {
                // "You must have exercises in your routine. "
                return;
            }

            List<IExerciseSets> normalized = es_data_source.ExerciseSets.SaveExerciseSets(Database);

            Routine routine = new Routine(RoutineNameTextField.Text,
                   normalized);
            routine.Database = Database;

            Database.InsertOrUndeleteAndUpdate(routine);

            // set the category for all of the exercises in this routine
            foreach (IExerciseSets ex_sets in normalized)
            {
                Exercise ex = ex_sets.Exercise;
                ex.Category = routine.Name;
                Database.Update(ex);
            }

            ReturnRoutine(routine);
        }

        void ReturnRoutine(IRoutine routine)
        {
            // pass it up to parent
            ValueChosen?.Invoke(routine);

            NavigationController.PopViewController(true);
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

            ExerciseSetsTableView.ReloadData();
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