using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;

namespace POLift
{
    using Model;
    using Service;

    [Activity(Label = "GraphActivity")]
    public class GraphActivity : Activity
    {
        const int SelectExerciseRequestCode = 0;

        PlotView plot_view;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Graph);

            plot_view = new PlotView(this);
            plot_view.Background = Resources.GetDrawable(
                Resource.Color.white);

            if(savedInstanceState == null)
            {
                Intent result_intent = new Intent(this, typeof(SelectExerciseActivity));
                StartActivityForResult(result_intent, SelectExerciseRequestCode);
            }
            else
            {
                int exercise_id = savedInstanceState.GetInt("exercise_id");

                if (exercise_id != 0) InitializePlot(exercise_id);
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                int exercise_id = data.Extras.GetInt("exercise_id");

                if(exercise_id != 0)
                {
                    InitializePlot(exercise_id);
                }
            }
        }

        void InitializePlot(int exercise_id)
        {
            plot_view.Model = CreatePlotModel(exercise_id);

            this.AddContentView(plot_view,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent));
        }

        PlotModel CreatePlotModel(int exercise_id)
        {
            Exercise ex = POLDatabase.ReadByID<Exercise>(exercise_id);

            var plotModel = new PlotModel { Title = $"{ex.Name} One-Rep Max" };

            DateTimeAxis date_axis = new DateTimeAxis { Position = AxisPosition.Bottom };
            plotModel.Axes.Add(date_axis);
            //plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0,  });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left });

            var series1 = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.White,
                Background = OxyColor.FromArgb(255, 255, 255, 255)

            };

            var exercise_results = POLDatabase.Table<ExerciseResult>()
                .Where(ex_result => ex_result.ExerciseID == exercise_id)
                .OrderBy(ex_result => ex_result.Time);

            date_axis.AbsoluteMinimum = DateTimeAxis.ToDouble(exercise_results.First().Time);
            date_axis.AbsoluteMaximum = DateTimeAxis.ToDouble(exercise_results.Last().Time);


            // must average each day's 1RM
            DateTime last_date = DateTime.MinValue;
            int orm_sum = 0;
            int orm_count = 0;
            foreach (ExerciseResult ex_result in exercise_results)
            {
                int orm = Helpers.OneRepMax(ex_result.Weight, ex_result.RepCount);
                //System.Diagnostics.Debug.WriteLine($"orm({ex_result.Weight},{ex_result.RepCount}) = {orm}");

                if (last_date.Date == ex_result.Time.Date)
                {
                    orm_sum += orm;
                    orm_count++;
                }
                else
                {
                    if(orm_count > 0)
                    {
                        double last_date_d = DateTimeAxis.ToDouble(last_date);
                        series1.Points.Add(new DataPoint(last_date_d, orm_sum / orm_count));
                        //System.Diagnostics.Debug.WriteLine($"{orm_sum}/{orm_count}   {last_date}");
                    }

                    orm_sum = orm;
                    orm_count = 1;
                }

                //double date_time = DateTimeAxis.ToDouble(ex_result.Time);
                
                //System.Diagnostics.Debug.WriteLine($"orm({ex_result.Weight},{ex_result.RepCount}) = {orm}");
                //series1.Points.Add(new DataPoint(date_time, orm));

                last_date = ex_result.Time;
            }

            if(orm_count > 0)
            {
                double last_date_d = DateTimeAxis.ToDouble(last_date);
                series1.Points.Add(new DataPoint(last_date_d, orm_sum / orm_count));
            }

            plotModel.Series.Add(series1);

            return plotModel;
        }
    }
}