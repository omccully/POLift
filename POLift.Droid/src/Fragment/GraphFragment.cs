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
using Android.Preferences;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;

using Microsoft.Practices.Unity;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Core.Model;
    using Core.Service;
    using Core.Helpers;
    using Core.ViewModel;

    public class GraphFragment : Fragment
    {
        private OrmGraphViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.OrmGraph; 
            }
        }

        const int SelectExerciseGroupRequestCode = 6;

        PlotView plot_view;
        TextView GraphDataTextView;

        //string ExerciseIDs = null;
        //string ExerciseGroupName = null;
        ExerciseName exercise_name_group = new ExerciseName("", "");

        const string ExerciseGroupNameKey = "exercise_group_name";
        const string ExerciseIDsKey = "exercise_ids";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            //Database = C.ontainer.Resolve<IPOLDatabase>();
            //orm_graph = new OrmGraph(Database);


            /*ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this.Activity);

            if(prefs.GetBoolean("exercise_created_since_last_difficulty_regeneration", true))
            {
                ExerciseDifficulty.Regenerate(Database);

                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutBoolean("exercise_created_since_last_difficulty_regeneration", false);
                editor.Apply();
            }*/

            plot_view = new PlotView(this.Activity);
            //plot_view.Background = Resources.GetDrawable(
             //   Resource.Color.white);

            plot_view.Background = Resources.GetDrawable(
                Resource.Color.background_material_light);


            //plot_view.Foreground = Resources.GetDrawable(
            //    Resource.Color.background_material_dark);

            //plot_view.
            exercise_name_group.Name = (savedInstanceState == null ? "" :
                savedInstanceState.GetString(ExerciseGroupNameKey, ""));
            exercise_name_group.ExerciseIDs = (savedInstanceState == null ? "" :
                savedInstanceState.GetString(ExerciseIDsKey, ""));

            // ExerciseGroupName = (savedInstanceState == null ? null :
            //     savedInstanceState.GetString("exercise_group_name"));
            // ExerciseIDs = (savedInstanceState == null ? null :
            //     savedInstanceState.GetString("exercise_ids"));

            if (String.IsNullOrWhiteSpace(exercise_name_group.ExerciseIDs))
            {
                Intent result_intent = new Intent(this.Activity, 
                    typeof(SelectExerciseGroupActivity));
                StartActivityForResult(result_intent, SelectExerciseGroupRequestCode);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(ExerciseGroupNameKey, exercise_name_group.Name);
            outState.PutString(ExerciseIDsKey, exercise_name_group.ExerciseIDs);
           
            base.OnSaveInstanceState(outState);
        }

        FrameLayout frame_layout = null;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (this.View != null) return this.View;

            View result = inflater.Inflate(Resource.Layout.Graph, container, false);
            frame_layout = result.FindViewById<FrameLayout>(Resource.Id.graph_frame);
            GraphDataTextView = result.FindViewById<TextView>(Resource.Id.GraphDataTextView);

            InitializePlot(exercise_name_group);

            return result;
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseGroupRequestCode)
            {
                exercise_name_group.Name = data.Extras.GetString(ExerciseGroupNameKey);
                exercise_name_group.ExerciseIDs = data.Extras.GetString(ExerciseIDsKey);

                InitializePlot(exercise_name_group);
                // OnCreateView should be called after OnActivityResult
                // thus creating the correct view based on ExerciseID
            }
        }

        void InitializePlot(IExerciseGroup exercise_group)
        {
            if (String.IsNullOrWhiteSpace(exercise_group.ExerciseIDs)) return;
            if (frame_layout == null || plot_view.Parent != null) return;

            Vm.InitializePlot(exercise_group);

            OxyColor android_bg = OxyColor.FromArgb(0xff, 0x30, 0x30, 0x30);

            PlotModel model = Vm.PlotModel;
            model.TitleColor = OxyColors.White;
            model.Background = android_bg;
            foreach(OxyPlot.Axes.Axis axis in model.Axes)
            {
                axis.TextColor = OxyColors.White;
            }

            foreach(Series series in model.Series)
            {
                series.Background = android_bg;
            }

            plot_view.Model = model;

            frame_layout.RemoveAllViews();
            frame_layout.AddView(plot_view);

            // generate list of values for sidebar
            GraphDataTextView.Text = Vm.DataText;
        }
    }
}