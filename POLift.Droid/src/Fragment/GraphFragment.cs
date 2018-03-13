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

        const int SelectExerciseDifficultyRequestCode = 0;

        PlotView plot_view;
        TextView GraphDataTextView;

        int ExerciseDifficultyID = 0;

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
            plot_view.Background = Resources.GetDrawable(
                Resource.Color.white);

            ExerciseDifficultyID = (savedInstanceState == null ? 0 :
                savedInstanceState.GetInt("exercise_difficulty_id"));

            if (ExerciseDifficultyID == 0)
            {
                Intent result_intent = new Intent(this.Activity, typeof(SelectExerciseDifficultyActivity));
                StartActivityForResult(result_intent, SelectExerciseDifficultyRequestCode);
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            if (ExerciseDifficultyID != 0)
            {
                outState.PutInt("exercise_difficulty_id", ExerciseDifficultyID);
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

            InitializePlot(ExerciseDifficultyID);

            return result;
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == SelectExerciseDifficultyRequestCode)
            {
                int exercise_difficulty_id = data.Extras.GetInt("exercise_difficulty_id");

                if (exercise_difficulty_id > 0)
                {
                    ExerciseDifficultyID = exercise_difficulty_id;
                    InitializePlot(ExerciseDifficultyID);
                    // OnCreateView should be called after OnActivityResult
                    // thus creating the correct view based on ExerciseID
                }
            }
        }

        void InitializePlot(int exercise_difficulty_id)
        {
            if (exercise_difficulty_id > 0 && frame_layout != null
                && plot_view.Parent == null)
            { 
                Vm.InitializePlot(exercise_difficulty_id);

                plot_view.Model = Vm.PlotModel;

                frame_layout.RemoveAllViews();
                frame_layout.AddView(plot_view);

                // generate list of values for sidebar
                GraphDataTextView.Text = Vm.DataText;
            }
        }
    }
}