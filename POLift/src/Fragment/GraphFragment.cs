﻿using System;
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

        IPOLDatabase Database;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            Database = C.ontainer.Resolve<IPOLDatabase>();

            plot_view = new PlotView(this.Activity);
            plot_view.Background = Resources.GetDrawable(
                Resource.Color.white);

            int exercise_id = (savedInstanceState == null ? 0 : 
                savedInstanceState.GetInt("exercise_id"));

            if (exercise_id == 0)
            {
                Intent result_intent = new Intent(this.Activity, typeof(SelectExerciseActivity));
                StartActivityForResult(result_intent, SelectExerciseRequestCode);
            }
            else
            {
                InitializePlot(exercise_id);
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


        FrameLayout frame_layout;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (this.View != null) return this.View;

            View result = inflater.Inflate(Resource.Layout.Graph, container, false);
            frame_layout = result.FindViewById<FrameLayout>(Resource.Id.graph_frame);

            // Use this to return your custom view for this Fragment
            return result;

            //return base.OnCreateView(inflater, container, savedInstanceState);
        }

        int ExerciseID = 0;
        void InitializePlot(int exercise_id)
        {
            ExerciseID = exercise_id;
            plot_view.Model = CreatePlotModel(exercise_id);
            frame_layout.AddView(plot_view);
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseRequestCode)
            {
                int exercise_id = data.Extras.GetInt("exercise_id");

                if (exercise_id != 0)
                {
                    InitializePlot(exercise_id);
                }
            }
        }

        PlotModel CreatePlotModel(int exercise_id)
        {
            Exercise ex = Database.ReadByID<Exercise>(exercise_id);

            var plotModel = new PlotModel { Title = $"{ex.Name} one-rep max" };

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

            var exercise_results = Database.Table<ExerciseResult>()
                .Where(ex_result => ex_result.ExerciseID == exercise_id && !ex_result.Deleted)
                .OrderBy(ex_result => ex_result.Time);

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
                //System.Diagnostics.Debug.WriteLine($"orm({ex_result.Weight},{ex_result.RepCount}) = {orm}");

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

            if (orm_count > 0)
            {
                double last_date_d = DateTimeAxis.ToDouble(last_date);
                series1.Points.Add(new DataPoint(last_date_d, orm_sum / orm_count));
            }
        }
    }
}