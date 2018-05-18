using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace POLift.Core.Helpers
{
    using Model;
    using Service;

    public class OrmGraph
    {
        IPOLDatabase Database;

        public OrmGraph(IPOLDatabase database)
        {
            this.Database = database;
        }

        public IEnumerable<ExerciseResult> GetPlotData(int exercise_difficulty_id)
        {
            return GetPlotData(Database.ReadByID<ExerciseDifficulty>(exercise_difficulty_id));
        }

        public IEnumerable<ExerciseResult> GetPlotData(IExerciseDifficulty exercise_difficulty)
        {
            return GetPlotData(exercise_difficulty.ExerciseIDs.ToIDIntegers());
        }

        public IEnumerable<ExerciseResult> GetPlotData(IEnumerable<int> exercise_ids)
        {
            // TODO: SQL builder for SQL OR operations for ExerciseID = __ OR ...

            return Database.Table<ExerciseResult>()
                .Where(ex_result => exercise_ids.Contains(ex_result.ExerciseID) && !ex_result.Deleted)
                .OrderBy(ex_result => ex_result.Time);
        }

        public PlotModel CreatePlotModel(IExerciseDifficulty exercise,
            IEnumerable<ExerciseResult> exercise_results)
        {
            return CreatePlotModel(exercise.Name, exercise_results);
        }

        public PlotModel CreatePlotModel(string name,
            IEnumerable<ExerciseResult> exercise_results)
        {
            var plotModel = new PlotModel { Title = $"{name} one-rep max" };

            DateTimeAxis date_axis = new DateTimeAxis { Position = AxisPosition.Bottom };
            plotModel.Axes.Add(date_axis);

            LinearAxis weight_axis = new LinearAxis { Position = AxisPosition.Left};
            plotModel.Axes.Add(weight_axis);

            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Background = OxyColor.FromArgb(255, 255, 255, 255)
            };


            if (exercise_results.Count() > 1)
            {
                DateTime min_date = exercise_results.First().Time;
                date_axis.AbsoluteMinimum = DateTimeAxis.ToDouble(min_date);

                DateTime max_date = exercise_results.Last().Time;
                date_axis.AbsoluteMaximum = DateTimeAxis.ToDouble(max_date);
                date_axis.MinimumRange = date_axis.AbsoluteMaximum - date_axis.AbsoluteMinimum;

                AddExerciseResultsToSeries(series1, exercise_results);
            }

            plotModel.Series.Add(series1);

            return plotModel;
        }

        public void AddExerciseResultsToSeries(LineSeries series1, 
            IEnumerable<ExerciseResult> exercise_results, bool only_up=false)
        {
            // must average each day's 1RM
            DateTime last_date = DateTime.MinValue;
            int orm_sum = 0;
            int orm_count = 0;

            int highest_orm = 0;

            foreach (ExerciseResult ex_result in exercise_results)
            {
                int orm = Helpers.OneRepMax(ex_result.Weight, ex_result.RepCount);

                if (last_date.Date == ex_result.Time.Date)
                {
                    orm_sum += orm;
                    orm_count++;
                }
                else
                {
                    if (orm_count > 0)
                    {
                        double last_date_d = DateTimeAxis.ToDouble(last_date);
                        int orm_average = orm_sum / orm_count;

                        if (!only_up)
                        {
                            series1.Points.Add(new DataPoint(last_date_d, orm_average));
                        }
                        else if(orm_average >= highest_orm)
                        {
                            series1.Points.Add(new DataPoint(last_date_d, orm_average));
                            highest_orm = orm_average;
                        }
                    }

                    orm_sum = orm;
                    orm_count = 1;
                }

                last_date = ex_result.Time;
            }

            if (orm_count > 0)
            {
                double last_date_d = DateTimeAxis.ToDouble(last_date);
                series1.Points.Add(new DataPoint(last_date_d, orm_sum / orm_count));
            }
        }

        public static string DataSourceText(IEnumerable<ExerciseResult> exercise_results)
        {
            StringBuilder data_text = new StringBuilder();
            DateTime last_date = DateTime.MinValue;
            foreach (ExerciseResult ex_result in exercise_results)
            {
                if (last_date.Date != ex_result.Time.Date)
                {
                    data_text.AppendLine();
                }

                data_text.Append(ex_result.Weight);
                data_text.Append(" weight, ");
                data_text.Append(ex_result.RepCount);
                data_text.Append(" reps, ");
                data_text.Append(ex_result.Time.ToString("d"));

                data_text.AppendLine();

                last_date = ex_result.Time;
            }
            return data_text.ToString();
        } 
    }
}
