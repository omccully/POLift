using Foundation;
using System.Collections.Generic;
using System;
using UIKit;
using POLift.Core.Service;
using POLift.Core.Model;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class CreateExerciseController : DatabaseController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private CreateExerciseViewModel Vm
        {
            get
            {
                return Application.Locator.CreateExercise;
            }
        }


        public CreateExerciseController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateExerciseButton.SetCommand(Vm.CreateExerciseCommand);

            MathTypePicker.Model = new MathTypePickerViewModel();
            
            MathTypePicker.Delegate = new MathTypePickerDelegate();

            ExerciseNameTextField.EditingChanged += (s, e) => { };

            bindings.Add(this.SetBinding(
                () => ExerciseNameTextField.Text,
                () => Vm.ExerciseNameInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => RepCountTextField.Text,
                () => Vm.RepCountInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => WeightIncrementTextField.Text,
                () => Vm.WeightIncrementInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => RestPeriodTextField.Text,
                () => Vm.RestPeriodInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => ConsecutiveSetsTextField.Text,
                () => Vm.ConsecutiveSetsInput,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            ExerciseNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RepCountTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            WeightIncrementTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RestPeriodTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            ConsecutiveSetsTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
        }

        class MathTypePickerDelegate : UIPickerViewDelegate
        {
            public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            UILabel label = view as UILabel;

            if (label == null)
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