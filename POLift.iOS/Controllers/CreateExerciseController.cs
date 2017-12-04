using Foundation;
using System;
using UIKit;
using POLift.Core.Service;
using POLift.Core.Model;

namespace POLift.iOS.Controllers
{
    public partial class CreateExerciseController : DatabaseController, IValueReturner<IExercise>
    {
        public event Action<IExercise> ValueChosen;

        public CreateExerciseController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateExerciseButton.TouchUpInside += CreateExerciseButton_TouchUpInside;

            MathTypePicker.Model = new MathTypePickerViewModel();
            
            MathTypePicker.Delegate = new MathTypePickerDelegate();


            ExerciseNameTextField.ShouldReturn = KeyboardDismisser;
            RepCountTextField.ShouldReturn = KeyboardDismisser;
            WeightIncrementTextField.ShouldReturn = KeyboardDismisser;
            RestPeriodTextView.ShouldReturn = KeyboardDismisser;
            ConsecutiveSetsTextView.ShouldReturn = KeyboardDismisser;
        }

        bool KeyboardDismisser(UITextField text_field)
        {
            text_field.ResignFirstResponder();
            return true;
        }

        private void CreateExerciseButton_TouchUpInside(object sender, EventArgs e)
        {
            try
            {
                string name = ExerciseNameTextField.Text;
                int max_reps = Int32.Parse(RepCountTextField.Text);
                float weight_increment = Single.Parse(WeightIncrementTextField.Text);
                int rest_period_s = Int32.Parse(RestPeriodTextView.Text);
                int consecutive_sets = Int32.Parse(ConsecutiveSetsTextView.Text);

                nint pm_index = MathTypePicker.SelectedRowInComponent(0);
                IPlateMath plate_math = PlateMath.PlateMathTypes[pm_index];

                Exercise ex = new Exercise(name, max_reps, weight_increment,
                    rest_period_s, plate_math);
                ex.ConsecutiveSetsForWeightIncrease = consecutive_sets;
                ex.Database = this.Database;

                Database.InsertOrUndeleteAndUpdate(ex);

                //SavePreferences();

                ReturnExercise(ex);
            }
            catch (FormatException)
            {
                // FormatException for int parsing
                // "Numerical fields must be integers",

            }
            catch (ArgumentException ae)
            {
                // ArgumentException for Exercise constructor
                // ae.Message, 
            }
        }

        void ReturnExercise(IExercise exercise)
        {
            // pass it up to parent
            ValueChosen?.Invoke(exercise);
            DismissViewController(true, delegate { });
        }

        class MathTypePickerDelegate : UIPickerViewDelegate
        {
            public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
            {
                UILabel label = view as UILabel;

                if(label == null)
                {
                    label = new UILabel();

                    label.TextAlignment = UITextAlignment.Center;
                    label.AdjustsFontSizeToFitWidth = true;
                }

                label.Text = pickerView.Model.GetTitle(pickerView, row, component);

                return label;
            }
        }

        class MathTypePickerViewModel : UIPickerViewModel
        {
            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return PlateMath.PlateMathTypes.Length;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                PlateMath pm = PlateMath.PlateMathTypes[row];
                if(pm == null)
                {
                    return "None";
                }
                else
                {
                    return pm.ToString();
                }
            }
        }
    }
}