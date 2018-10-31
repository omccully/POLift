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
using Android.Support.Design.Widget;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Provider;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Xamarin.Android;
using System.IO;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.Droid
{
    using Core.Model;
    using Core.Service;
    using Core.Helpers;
    using Core.ViewModel;
    using Service;

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
            //Database = C.Database
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

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this.Activity),
                ViewModelLocator.Default.KeyValueStorage);

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
        ViewGroup filters_group = null;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (this.View != null) return this.View;

            View result = inflater.Inflate(Resource.Layout.Graph, container, false);
            frame_layout = result.FindViewById<FrameLayout>(Resource.Id.graph_frame);
            GraphDataTextView = result.FindViewById<TextView>(Resource.Id.GraphDataTextView);
            filters_group = result.FindViewById<ViewGroup>(Resource.Id.graph_filters);
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


        static MultiExerciseSelector ExSelector = null;
        static string last_exercise_ids = null;
        IEnumerable<IExercise> exercises = null;

        void InitializePlot(IExerciseGroup exercise_group)
        {
            if (String.IsNullOrWhiteSpace(exercise_group.ExerciseIDs)) return;
            if (frame_layout == null || plot_view.Parent != null) return;


            if(ExSelector == null || last_exercise_ids != exercise_group.ExerciseIDs)
            {
                exercises = exercise_group.ExerciseIDs
                    .ToIDIntegers().Select(id => Vm.Database.ReadByID<Exercise>(id))
                    .Cast<IExercise>();
                ExSelector = new MultiExerciseSelector(exercises);
                ExSelector.CheckedChanged += ExSelector_CheckedChanged;
            }

            InitializePlotOfSelected();

            last_exercise_ids = exercise_group.ExerciseIDs;
        }

        private void ExSelector_CheckedChanged(object sender, CheckBoxStateChangeEventArgs e)
        {
            // display loading
            e.CheckBox.Text = "Loading...";
            //e.CheckBox.Visibility = ViewStates.Gone;
            //e.CheckBox.Visibility = ViewStates.Visible;
            // e.CheckBox.Invalidate();
            this.View.Post(delegate
            {
                InitializePlotOfSelected();
            });
        }

        void InitializePlotOfSelected()
        {
            System.Diagnostics.Debug.WriteLine("InitializePlotOfSelected()");
            ExerciseName exercise_group = new ExerciseName(exercise_name_group.Name, 
                ExSelector.ExerciseIDs);

            Vm.InitializePlot(exercise_group);

            OxyColor android_bg = OxyColor.FromArgb(0xff, 0x30, 0x30, 0x30);

            PlotModel model = Vm.PlotModel;
            model.TitleColor = OxyColors.White;
            model.Background = android_bg;
            foreach (OxyPlot.Axes.Axis axis in model.Axes)
            {
                axis.TextColor = OxyColors.White;
            }

            foreach (Series series in model.Series)
            {
                series.Background = android_bg;
            }

            plot_view.Model = model;

            frame_layout.RemoveAllViews();
            frame_layout.AddView(plot_view);

            // generate list of values for sidebar
            GraphDataTextView.Text = Vm.DataText;

            filters_group.RemoveAllViews();
            ExSelector.AddViews(filters_group);


            View view = this.Activity.LayoutInflater.Inflate(Resource.Layout.FloatingShareButton, frame_layout);

            //System.Diagnostics.Debug.WriteLine(view.GetType());
            FloatingActionButton fab = view.FindViewById<FloatingActionButton>(Resource.Id.floating_share_button);
            //FloatingActionButton fab = view as FloatingActionButton;

            if (fab != null)
            {
                System.Diagnostics.Debug.WriteLine("fab != null");
                fab.Click += Fab_Click;
                //frame_layout.AddView(fab);


            }
            else
            {
                System.Diagnostics.Debug.WriteLine("fab == null");
            }

        }



        private void Fab_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Fab_Click");
            if (plot_view == null) return;

            Intent i = new Intent(Intent.ActionSend);

            i.SetType("image/png");
            System.IO.Stream stream = new MemoryStream();

            i.PutExtra(Intent.ExtraStream, GetImageUri(this.Activity, ScreenshotView(plot_view)));

            this.Activity.StartActivity(Intent.CreateChooser(i, "Share this via"));
        }

        

        public Android.Net.Uri GetImageUri(Context inContext, Bitmap inImage)
        {
            System.IO.Stream bytes = new MemoryStream();
            //inImage.Compress(Bitmap.CompressFormat.Jpeg, 100, bytes);
          
            string path = MediaStore.Images.Media.InsertImage(inContext.ContentResolver, inImage, "1rm Graph", "1 rep max over time");
            return Android.Net.Uri.Parse(path);
        }

        public static Bitmap ScreenshotView(View view)
        {

            Bitmap returnedBitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);

            Canvas canvas = new Canvas(returnedBitmap);

            Drawable bgDrawable = view.Background;
            if (bgDrawable != null)
                bgDrawable.Draw(canvas);
            else
                canvas.DrawColor(Color.White);
            
            view.Draw(canvas);

            Paint p = new Paint();
            p.Color = Color.Blue;
            p.SetStyle(Paint.Style.Fill);
            p.TextSize = 26;
            p.UnderlineText = true;

            //canvas.DrawText("polift-app.com", 15, 47, p);
            canvas.DrawText("polift-app.com", 80, 105, p);
            

            return returnedBitmap;
        }
    }
}