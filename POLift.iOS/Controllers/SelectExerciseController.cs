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
    public partial class SelectExerciseController : DatabaseController
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
        }

        void RefreshExerciseList()
        {
            eds = new ExercisesInCategoriesDataSource(Vm.ExercisesInCategories);

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


            //eds.ExerciseDelete += Eds_ExerciseDelete;
            ExercisesTableView.Source = eds;
            ExercisesTableView.ReloadData();
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
            public override string[] SectionIndexTitles(UITableView tableView)
            {
                if (_SectionIndexTitles == null)
                {
                    _SectionIndexTitles = ExerciseCategories
                        .Select(ec => ec.Name).ToArray();
                }

                return _SectionIndexTitles;
            }


            public override string TitleForHeader(UITableView tableView, nint section)
            {
                return SectionIndexTitles(tableView)[section];
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



        /* class ExercisesInCategoriesDataSource : UITableViewSource, IValueReturner<IExercise>
         {
             public static NSString ExerciseInCategoriesCellId = new NSString("EditDeleteTableViewCell");

             public event Action<IExercise> ValueChosen;
             public event Action<IExercise> ExerciseDelete;
             public event Action<IExercise> ExerciseEdit;

             List<KeyValuePair<string, List<IExercise>>> ExercisesInCategories;

             public ExercisesInCategoriesDataSource(List<KeyValuePair<string, List<IExercise>>> exercises_in_categories)
             {
                 this.ExercisesInCategories = exercises_in_categories;
             }

             public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
             {
                 UITableViewCell cell = tableView.DequeueReusableCell("exercise_cell");
                 //tableView.Cell

                 ExerciseCell rcell = cell as ExerciseCell;

                 //cell.TextLabel.Lines = 3;
                 //cell.TextLabel.Text = PathToExercise(indexPath).ToString();

                 //rcell.ExerciseLabel.Text = PathToExercise(indexPath).ToString();
                 //rcell.Update(PathToExercise(indexPath));

                 IExercise exercise = PathToExercise(indexPath);
                 rcell.Setup(exercise, 
                 delegate // edit
                 {
                     ExerciseEdit?.Invoke(exercise);
                 },
                 delegate // delete
                 {
                     ExerciseDelete?.Invoke(exercise);
                 });

                 return cell;
             }

             public override nint NumberOfSections(UITableView tableView)
             {
                 return ExercisesInCategories.Count;
             }

             string[] _SectionIndexTitles;
             public override string[] SectionIndexTitles(UITableView tableView)
             {
                 if(_SectionIndexTitles == null)
                 {
                     _SectionIndexTitles = ExercisesInCategories.Select(ec => ec.Key).ToArray();
                 }

                 return _SectionIndexTitles;
             }


             public override string TitleForHeader(UITableView tableView, nint section)
             {
                 return SectionIndexTitles(tableView)[section];
             }

             public override nint RowsInSection(UITableView tableview, nint section)
             {
                 return ExercisesInCategories[(int)section].Value.Count;
             }

             public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
             {
                 Console.WriteLine($"calling parent.ReturnExercise(exercises[{indexPath.Row}])");

                 ValueChosen?.Invoke(PathToExercise(indexPath));
             }


             IExercise PathToExercise(NSIndexPath indexPath)
             {
                 return ExercisesInCategories[indexPath.Section].Value[indexPath.Row];
             }
         }





         /* class ExercisesDataSource : UITableViewSource, IValueReturner<IExercise>
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
  }*/
    }
}