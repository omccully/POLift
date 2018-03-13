using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Droid
{
    using Service;
    using Core.Model;
    using Core.Service;

    [Activity(Label = "Edit routine result")]
    public class EditRoutineResultActivity : Activity
    {
        IPOLDatabase Database;
        RoutineResult _RoutineResult;
        LinearLayout Layout;
        Button DoneEditingRoutineResultButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.EditRoutineResult);
            Layout = 
                FindViewById<LinearLayout>(Resource.Id.edit_routine_result_layout);
            DoneEditingRoutineResultButton = 
                FindViewById<Button>(Resource.Id.DoneEditingRoutineResultButton);

            DoneEditingRoutineResultButton.Click += DoneEditingRoutineResultButton_Click;

            int routine_result_id = Intent.GetIntExtra("routine_result_id", -1);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            if(routine_result_id > 0)
            {
                _RoutineResult = Database.ReadByID<RoutineResult>(routine_result_id);
                InitializeGUI();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("invalid routine ID " + routine_result_id);
                AndroidHelpers.DisplayError(this, "Error: invalid routine result", delegate
                {
                    Finish();
                });
            }
        }

        private void DoneEditingRoutineResultButton_Click(object sender, EventArgs e)
        {
            _RoutineResult.SaveEdits(WeightEdits, RepsEdits);
            SetResult(Result.Ok);
            Finish();
        }

        TextView Label(string str)
        {
            TextView text_view = new TextView(this);
            text_view.Text = str;
            return text_view;
        }

        Dictionary<int, float> WeightEdits = new Dictionary<int, float>();
        Dictionary<int, int> RepsEdits = new Dictionary<int, int>();

        void InitializeGUI()
        {
            Layout.AddView(Label(_RoutineResult.TimeDetails));

            int last_exercise_id = 0;
            foreach (IExerciseResult ex_result in _RoutineResult.ExerciseResults)
            {
                if(last_exercise_id != ex_result.ExerciseID)
                {
                    Layout.AddView(Label(System.Environment.NewLine + 
                        ex_result.Exercise.ToString()));
                }

                Layout.AddView(EditLayoutForExerciseResult(ex_result));

                last_exercise_id = ex_result.ExerciseID;
            }
        }

        LinearLayout EditLayoutForExerciseResult(IExerciseResult ex_result)
        {
            LinearLayout ex_edit_layout = new LinearLayout(this/*,
                    new ViewGroup.LayoutParams(
                        ViewGroup.LayoutParams.FillParent,
                        ViewGroup.LayoutParams.WrapContent)*/);
            ex_edit_layout.Orientation = Orientation.Horizontal;

            ex_edit_layout.AddView(Label("Weight = "));

            EditText weight_edit = new EditText(this);
            weight_edit.InputType = Android.Text.InputTypes.ClassNumber;
                //Android.Text.InputTypes.NumberFlagDecimal;
            weight_edit.Text = ex_result.Weight.ToString();
            
            weight_edit.TextChanged += delegate
            {
                try
                {
                    WeightEdits[ex_result.ID] =
                        Single.Parse(weight_edit.Text);
                }
                catch (FormatException)
                {

                }
            };

            ex_edit_layout.AddView(weight_edit);

            ex_edit_layout.AddView(Label(", Reps = "));

            EditText reps_edit = new EditText(this);
            reps_edit.InputType = Android.Text.InputTypes.ClassNumber;
            reps_edit.Text = ex_result.RepCount.ToString();
            reps_edit.TextChanged += delegate
            {
                try
                {
                    RepsEdits[ex_result.ID] =
                        Int32.Parse(reps_edit.Text);
                }
                catch (FormatException)
                {

                }
            };
            ex_edit_layout.AddView(reps_edit);

            return ex_edit_layout;
        }
    }
}
