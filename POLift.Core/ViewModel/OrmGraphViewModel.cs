using GalaSoft.MvvmLight.Views;
using POLift.Core.Model;
using POLift.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using GalaSoft.MvvmLight;
using POLift.Core.Helpers;

namespace POLift.Core.ViewModel
{
    public class OrmGraphViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;
        OrmGraph orm_graph;

        public IValueReturner<IExerciseDifficulty> SelectExerciseDifficultyViewModel;

        public OrmGraphViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
            orm_graph = new OrmGraph(database);
        }

        public void PromptUser()
        {
            SelectExerciseDifficultyViewModel.ValueChosen += 
                SelectExerciseDifficultyViewModel_ValueChosen;

            navigationService.NavigateTo(
                ViewModelLocator.SelectExerciseDifficultyPageKey);
        }

        PlotModel _PlotModel;
        public PlotModel PlotModel
        {
            get
            {
                return _PlotModel;
            }
            set
            {
                Set(ref _PlotModel, value);
            }
        }

        string _DataText;
        public string DataText
        {
            get
            {
                return _DataText;
            }
            set
            {
                Set(ref _DataText, value);
            }
        }

        private void SelectExerciseDifficultyViewModel_ValueChosen(IExerciseDifficulty ed)
        {
            SelectExerciseDifficultyViewModel.ValueChosen -= 
                SelectExerciseDifficultyViewModel_ValueChosen;

            InitializePlot(ed);
        }

        public void InitializePlot(int ed_id)
        {
            ExerciseDifficulty ed = Database.ReadByID<ExerciseDifficulty>(ed_id);

            InitializePlot(ed);
        }

        public void InitializePlot(IExerciseDifficulty ed)
        {
            IEnumerable<ExerciseResult> data = orm_graph.GetPlotData(ed);

            PlotModel = orm_graph.CreatePlotModel(ed, data);

            DataText = OrmGraph.DataSourceText(data);
        }
    }
}
