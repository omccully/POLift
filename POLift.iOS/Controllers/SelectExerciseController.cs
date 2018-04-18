using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using POLift.Core.Service;
using POLift.Core.Model;
using POLift.Core.ViewModel;

using POLift.iOS.DataSources;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class SelectExerciseController : UIViewController
    {
        private SelectExerciseViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SelectExercise;
            }
        }

        public SelectExerciseController (IntPtr handle) : base (handle)
        {
        }

        //ExercisesDataSource eds;
        ExercisesInCategoriesDataSource eds;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //ExerciseListTableView.RegisterClassForCellReuse(typeof(UITableViewCell),
            //    ExercisesDataSource.ExerciseCellId);

            //ExerciseListTableView.RegisterClassForCellReuse(typeof(EditDeleteTableViewCell),
            //   ExercisesInCategoriesDataSource.ExerciseInCategoriesCellId);

            Vm.ExercisesChanged += Vm_ExercisesChanged;

            CreateExerciseLink.TouchUpInside += delegate
            {
                Vm.NavigateCreateExercise();
            };

            //eds = new ExercisesDataSource(Vm.Exercises);

            ExercisesTableView.RowHeight = UITableView.AutomaticDimension;
            ExercisesTableView.EstimatedRowHeight = 40f;
            
            RefreshExerciseList();

            NSIndexPath path = eds.FirstRowInSection(Vm.Category);
            if(path != null)
            {
                Console.WriteLine("path = " + path);
                
                this.BeginInvokeOnMainThread(delegate
                {
                    ExercisesTableView.ScrollToRow(path,
                        UITableViewScrollPosition.Top, false);
                });
            }
        }

        void RefreshExerciseList()
        {
            List<ExerciseCategory> categories = Vm.ExercisesInCategories;
            eds = new ExercisesInCategoriesDataSource(categories);

            eds.EditClicked += delegate (IExercise exercise) 
            {
                Vm.EditExercise(exercise);
            };

            eds.DeleteClicked += delegate (IExercise exercise, Action action)
            {
                Vm.DeleteExercise(exercise, action);
            };

            eds.RowClicked += delegate(object sender, IExercise exercise)
            {
                Vm.SelectExercise(exercise);
            };

            ExercisesTableView.Source = eds;
            //ExercisesTableView.SectionHeaderHeight = 5;
           // ExercisesTableView.Section
            //ExercisesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            ExercisesTableView.ReloadData();

            if (categories.Sum(ec => ec.Exercises.Count()) == 0)
            {
                // if no exercises, just show the create exercise page

                Vm.NavigateCreateExercise();
            }
        }


        private void Vm_ExercisesChanged(object sender, EventArgs e)
        {
            RefreshExerciseList();
            Console.WriteLine("call RefreshExerciseList()");
        }

        class ExercisesInCategoriesDataSource : UITableViewSource
        {
            public event Action<IExercise> EditClicked;
            public event Action<IExercise, Action> DeleteClicked;

            public event EventHandler<IExercise> RowClicked;

            IList<ExerciseCategory> ExerciseCategories;

            public ExercisesInCategoriesDataSource(IList<ExerciseCategory> data)
            {
                this.ExerciseCategories = data;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell("exercise_cell");

                ExerciseCell rcell = cell as ExerciseCell;

                IExercise exercise = IndexPathToExercise(indexPath);
                rcell.Setup(exercise,
                    delegate // edit
                    {
                        EditClicked?.Invoke(exercise);
                    });

                return cell;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return ExerciseCategories.Count;
            }

            string[] _SectionIndexTitles;
            public string[] sit(UITableView tableView=null)
            {
                if (_SectionIndexTitles == null)
                {
                    _SectionIndexTitles = ExerciseCategories
                        .Select(ec => ec.Name).ToArray();
                }

                return _SectionIndexTitles;
            }

            public NSIndexPath FirstRowInSection(string category)
            {
                int index = Array.IndexOf(sit(), category);
                Console.WriteLine(category + " index = " + index);
                if(index != -1)
                {
                    return NSIndexPath.FromRowSection(0, index);
                }
                return null;
            }

            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return sit(tableView)[section];
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return ExerciseCategories[(int)section].Exercises.Count;
            }

            IExercise IndexPathToExercise(NSIndexPath indexPath)
            {
                return ExerciseCategories[indexPath.Section]
                    .Exercises[indexPath.Row];
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                RowClicked?.Invoke(this, ExerciseCategories[indexPath.Section]
                    .Exercises[indexPath.Row]);
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override void CommitEditingStyle(UITableView tableView,
                UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                if (editingStyle == UITableViewCellEditingStyle.Delete)
                {
                    if (DeleteClicked != null)
                    {
                        List<IExercise> exercises_in_section = 
                            ExerciseCategories[indexPath.Section]
                            .Exercises;

                        int start_count = exercises_in_section.Count;
                        IExercise item = IndexPathToExercise(indexPath);
                        DeleteClicked(item, delegate
                        {
                            // ensure this delegate isn't called twice
                            // for when multiple event handlers are hooked up 
                            if (start_count == exercises_in_section.Count)
                            {
                                exercises_in_section.RemoveAt(indexPath.Row);
                                tableView.DeleteRows(new NSIndexPath[] { indexPath },
                                    UITableViewRowAnimation.Fade);
                            }
                        });
                    }
                }
            }
        }
    }
}