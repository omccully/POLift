using Foundation;
using System;
using UIKit;
using System.Linq;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;
using POLift.Core.Model;
using System.Collections.Generic;

namespace POLift.iOS.Controllers
{
    public partial class SelectExerciseGroupController : UITableViewController
    {
        const string ExerciseGroupCellId = "exercise_group_cell";

        private SelectExerciseGroupViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SelectExerciseName;
            }
        }

        public SelectExerciseGroupController (IntPtr handle) : base (handle)
        {
        }

        List<ExerciseGroupCategory> _ExerciseGroupCategories = new List<ExerciseGroupCategory>();
        List<ExerciseGroupCategory> ExerciseGroupCategories
        {
            get
            {
                return _ExerciseGroupCategories;
            }
            set
            {
                _ExerciseGroupCategories = value;
                _SectionIndexTitles = null;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RegisterClassForCellReuse(typeof(UITableViewCell),
                ExerciseGroupCellId);

            ExerciseGroupCategories = Vm.ExerciseCategories;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return ExerciseGroupCategories.Count;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return ExerciseGroupCategories[(int)section]
                .ExerciseGroups.Count;
        }

        string[] _SectionIndexTitles = null;
        string[] SectionTitles
        {
            get
            {
                if (_SectionIndexTitles == null)
                {
                    _SectionIndexTitles = ExerciseGroupCategories
                        .Select(edc => edc.Name).ToArray();
                }

                return _SectionIndexTitles;
            }
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return SectionTitles[section];
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = TableView.DequeueReusableCell(ExerciseGroupCellId);

            cell.TextLabel.Text = GetEdFromIndexPath(indexPath).Name;

            return cell;
        }

        ExerciseGroupCategory GetEdcFromIndexPath(NSIndexPath indexPath)
        {
            return ExerciseGroupCategories[(int)indexPath.Section];
        }

        IExerciseGroup GetEdFromIndexPath(NSIndexPath indexPath)
        {
            return GetEdcFromIndexPath(indexPath)
                .ExerciseGroups[(int)indexPath.Row];
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            Vm.ReturnExerciseGroup(GetEdFromIndexPath(indexPath));
        }
    }
}