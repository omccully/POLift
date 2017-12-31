using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;
using POLift.Core.Model;

namespace POLift.iOS.Controllers
{
    public partial class SelectExerciseDifficultyController : UITableViewController
    {
        const string ExerciseDifficultyCellId = "exercise_difficulty_cell";

        private SelectExerciseDifficultyViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SelectExerciseDifficulty;
            }
        }

        public SelectExerciseDifficultyController (IntPtr handle) : base (handle)
        {
            ExerciseDifficultyCategories = new List<ExerciseDifficultyCategory>();
        }

        List<ExerciseDifficultyCategory> _ExerciseDifficultyCategories;
        List<ExerciseDifficultyCategory> ExerciseDifficultyCategories
        {
            get
            {
                return _ExerciseDifficultyCategories;
            }
            set
            {
                _ExerciseDifficultyCategories = value;
                _SectionIndexTitles = null;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExerciseDifficultyCellId);

            ExerciseDifficultyCategories = Vm.ExerciseDifficultyCategories;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return ExerciseDifficultyCategories.Count;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return ExerciseDifficultyCategories[(int)section]
                .ExerciseDifficulties.Count;
        }

        string[] _SectionIndexTitles = null;
        public override string[] SectionIndexTitles(UITableView tableView)
        {
            if (_SectionIndexTitles == null)
            {
                _SectionIndexTitles = ExerciseDifficultyCategories
                    .Select(edc => edc.Name).ToArray();
            }

            return _SectionIndexTitles;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = TableView.DequeueReusableCell(ExerciseDifficultyCellId);

            cell.TextLabel.Text = GetEdFromIndexPath(indexPath).ToString();
            
            return cell;
        }


        ExerciseDifficultyCategory GetEdcFromIndexPath(NSIndexPath indexPath)
        {
            return ExerciseDifficultyCategories[(int)indexPath.Section];
        }

        IExerciseDifficulty GetEdFromIndexPath(NSIndexPath indexPath)
        {
            return GetEdcFromIndexPath(indexPath)
                .ExerciseDifficulties[(int)indexPath.Row];
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Vm.ReturnExerciseDifficulty(GetEdFromIndexPath(indexPath));
        }
    }
}