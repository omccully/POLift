using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;

using Microsoft.Practices.Unity;

namespace POLift
{
    using Model;
    using Service;

    public class GraphFragment : Fragment
    {
        const int SelectExerciseRequestCode = 0;

        PlotView plot_view;
        TextView GraphDataTextView;

        IPOLDatabase Database;

        int ExerciseID = 0;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            Database = C.ontainer.Resolve<IPOLDatabase>();

            plot_view = new PlotView(this.Activity);
            plot_view.Background = Resources.GetDrawable(
                Resource.Color.white);

            ExerciseID = (savedInstanceState == null ? 0 : 
                savedInstanceState.GetInt("exercise_id"));

            if (ExerciseID == 0)
            {
                Intent result_intent = new Intent(this.Activity, typeof(SelectExerciseActivity));
                StartActivityForResult(result_intent, SelectExerciseRequestCode);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            if (ExerciseID != 0)
            {
                outState.PutInt("exercise_id", ExerciseID);
            }

            base.OnSaveInstanceState(outState);
        }


        FrameLayout frame_layout = null;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (this.View != null) return this.View;

            View result = inflater.Inflate(Resource.Layout.Graph, container, false);
            frame_layout = result.FindViewById<FrameLayout>(Resource.Id.graph_frame);
            GraphDataTextView = result.FindViewById<TextView>(Resource.Id.GraphDataTextView);

            InitializePlot(ExerciseID);

            return result;
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                int exercise_id = data.Extras.GetInt("exercise_id");

                if (exercise_id > 0)
                {
                    ExerciseID = exercise_id;
                    InitializePlot(ExerciseID);
                    // OnCreateView should be called after OnActivityResult
                    // thus creating the correct view based on ExerciseID
                }
            }
        }

        void InitializePlot(int exercise_id)
        {
            if (exercise_id > 0 && frame_layout != null)
            {
                Exercise ex = Database.ReadByID<Exercise>(exercise_id);
                IEnumerable<ExerciseResult> data = GetPlotData(exercise_id);
                plot_view.Model = CreatePlotModel(ex, data);
                frame_layout.RemoveAllViews();
                frame_layout.AddView(plot_view);

                StringBuilder data_text = new StringBuilder();

                DateTime last_date = DateTime.MinValue;
                foreach (ExerciseResult ex_result in data)
                {
                    if (last_date.Date != ex_result.Time.Date)
                    {
                        data_text.AppendLine();
                    }

                    data_text.Append(ex_result.Weight);
                    data_text.Append(" weight, ");
                    data_text.Append(ex_result.RepCount);
                    data_text.Append(" reps, ");
                    data_text.Append(ex_result.Time.ToShortDateString());

                    data_text.AppendLine();

                    last_date = ex_result.Time;
                }

                GraphDataTextView.Text = data_text.ToString();
            }
        }


        IEnumerable<ExerciseResult> GetPlotData(int exercise_id)
        {
            return Database.Table<ExerciseResult>()
                .Where(ex_result => ex_result.ExerciseID == exercise_id && !ex_result.Deleted)
                .OrderBy(ex_result => ex_result.Time);
        }

        PlotModel CreatePlotModel(IExercise exercise, 
            IEnumerable<ExerciseResult> exercise_results)
        {
            var plotModel = new PlotModel { Title = $"{exercise.Name} one-rep max" };

            DateTimeAxis date_axis = new DateTimeAxis { Position = AxisPosition.Bottom };
            plotModel.Axes.Add(date_axis);
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Background = OxyColor.FromArgb(255, 255, 255, 255)
            };

            
            if (exercise_results.Count() > 1)
            {
                date_axis.AbsoluteMinimum = DateTimeAxis.ToDouble(exercise_results.First().Time);
                date_axis.AbsoluteMaximum = DateTimeAxis.ToDouble(exercise_results.Last().Time);

                AddExerciseResultsToSeries(series1, exercise_results);
            }

            plotModel.Series.Add(series1);

            return plotModel;
        }

        void AddExerciseResultsToSeries(LineSeries series1, IEnumerable<ExerciseResult> exercise_results)
        {
            // must average each day's 1RM
            DateTime last_date = DateTime.MinValue;
            int orm_sum = 0;
            int orm_count = 0;
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
                        series1.Points.Add(new DataPoint(last_date_d, orm_sum / orm_count));
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
    }
}