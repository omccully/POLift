using Foundation;
using System;
using UIKit;
using POLift.Core.Model;
using POLift.Core.Service;
using System.Collections.Generic;

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
                return Application.Locator.CreateRoutine;
            }
        }

        public CreateRoutineController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ExerciseSetsTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExerciseSetsDataSource.ExerciseSetsCellId);

            RefreshExerciseSetsList();

            RoutineNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            AddExerciseLink.SetCommand(Vm.AddExerciseCommand);


            bindings.Add(this.SetBinding(
                () => RoutineNameTextField.Text,
                () => Vm.RoutineNameInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            CreateRoutineButton.SetCommand(Vm.CreateRoutineCommand);
            
            Vm.ExerciseSets.CollectionChanged += ExerciseSets_CollectionChanged;
        }

        private void ExerciseSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshExerciseSetsList();
        }

        void RefreshExerciseSetsList()
        {
            ExerciseSetsTableView.Source = 
                new ExerciseSetsDataSource(Vm.ExerciseSets);
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