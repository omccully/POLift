using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

using POLift.Core.Service;
using POLift.Core.Model;
using POLift.Core.ViewModel;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class SelectExerciseController : DatabaseController
    {
        private SelectExerciseViewModel Vm
        {
            get
            {
                return Application.Locator.SelectExercise;
            }
        }

        public SelectExerciseController (IntPtr handle) : base (handle)
        {
        }

        ExercisesDataSource eds;
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExerciseListTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExercisesDataSource.ExerciseCellId);

            eds = new ExercisesDataSource(Vm.Exercises);
            eds.ValueChosen += Eds_ValueChosen;
            ExerciseListTableView.Source = eds;

            CreateExerciseLink.SetCommand(Vm.CreateExerciseCommand);
        }

        private void Eds_ValueChosen(IExercise obj)
        {
            Vm.SelectExerciseCommand(obj).Execute(null);
        }

        class ExercisesDataSource : UITableViewSource, IValueReturner<IExercise>
        {
            public static NSString ExerciseCellId = new NSString("ExerciseCell");

            public event Action<IExercise> ValueChosen;

            List<Exercise> exercises;

            public ExercisesDataSource(IEnumerable<Exercise> exercises)
            {
                this.exercises = new List<Exercise>(exercises);
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell(ExerciseCellId);

                cell.TextLabel.Lines = 3;
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

                ValueChosen?.Invoke(exercises[indexPath.Row]);
            }
        }
    }
}