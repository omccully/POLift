using Foundation;
using System;
using UIKit;
using CoreGraphics;

using POLift.Core.Model;
using POLift.Core.ViewModel;
using POLift.Core.Helpers;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class EditRoutineResultController : UIViewController
    {
        private EditRoutineResultViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.EditRoutineResult;
            }
        }


        public EditRoutineResultController (IntPtr handle) : base (handle)
        {
           
        }

        nfloat width;
        const int line_height = 30;
        const int line_gap = 10;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            int y = 100;
            width = View.Bounds.Width;

            UIButton done_button = new UIButton(UIButtonType.System);
            done_button.Frame = new CGRect(20, y, 55, line_height);
            done_button.SetTitle("Done", UIControlState.Normal);
            done_button.SetCommand(Vm.DoneCommand);

            this.View.AddSubview(done_button);

            y += line_height + line_gap;

            UILabel time_label = new UILabel();
            time_label.Frame = new CGRect(20, y, 280, line_height);
            time_label.Text = Vm.TimeDetailsText;

            int last_exercise_id = 0;
            foreach (IExerciseResult ex_result in Vm.RoutineResult.ExerciseResults)
            {
                if (last_exercise_id != ex_result.ExerciseID)
                {
                    y += line_height + line_gap;
                    UILabel exercise_label = new UILabel();
                    exercise_label.Frame = new CGRect(20, y, width - 40, line_height);
                    exercise_label.Text = ex_result.Exercise.ToString();
                    this.View.AddSubview(exercise_label);
                }
                y += line_height + line_gap;

                AddEditLayoutForExerciseResult(ex_result, y);

                last_exercise_id = ex_result.ExerciseID;
            }
        }


        void AddEditLayoutForExerciseResult(IExerciseResult exercise_result, int y)
        {
            const int weight_label_width = 80;
            const int weight_text_box_width = 70;
            
            const int reps_label_width = 70;
            const int reps_text_box_width = 55;

            const int horizontal_gap = 5;

            int x = 20;

            UILabel weight_label = new UILabel();
            weight_label.Frame = new CGRect(x, y, weight_label_width, line_height);
            weight_label.Text = "Weight = ";
            this.View.AddSubview(weight_label);

            x += weight_label_width + horizontal_gap;

            UITextField weight_text_field = new UITextField();
            weight_text_field.BorderStyle = UITextBorderStyle.RoundedRect;
            weight_text_field.Frame = new CGRect(x, y, weight_text_box_width, line_height);
            weight_text_field.Text = exercise_result.Weight.ToString();

            weight_text_field.EditingChanged += delegate
            {
                try
                {
                    Vm.WeightEdits[exercise_result.ID] =
                        Single.Parse(weight_text_field.Text);
                }
                catch (FormatException)
                {

                }
            };

            this.View.AddSubview(weight_text_field);

            x += weight_text_box_width + horizontal_gap;

            UILabel reps_label = new UILabel();
            reps_label.Frame = new CGRect(x, y, reps_label_width, line_height);
            reps_label.Text = ", Reps = ";
            this.View.AddSubview(reps_label);

            x += reps_label_width + horizontal_gap;

            UITextField reps_text_field = new UITextField();
            reps_text_field.BorderStyle = UITextBorderStyle.RoundedRect;
            reps_text_field.Frame = new CGRect(x, y, reps_text_box_width, line_height);
            reps_text_field.Text = exercise_result.RepCount.ToString();

            reps_text_field.EditingChanged += delegate
            {
                try
                {
                    Vm.RepsEdits[exercise_result.ID] =
                        Int32.Parse(reps_text_field.Text);
                }
                catch (FormatException)
                {

                }
            };

            this.View.AddSubview(reps_text_field);
        }
    }
}