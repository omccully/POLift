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
    public partial class CreateExerciseController : UIViewController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private CreateExerciseViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.CreateExercise;
            }
        }

        public CreateExerciseController (IntPtr handle) : base (handle)
        {
        }

        public string SubmitButtonText
        {
            get
            {
                return CreateExerciseButton.Title(UIControlState.Normal);
            }
            set
            {
                CreateExerciseButton.SetTitle(value, UIControlState.Normal);
            }
        }
        MathTypePickerViewModel PickerVM;
        MathTypePickerDelegate PickerDelegate;
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateExerciseButton.SetCommand(Vm.CreateExerciseCommand);
            HelpButton.SetCommand(Vm.HelpCommand);

            PickerVM = new MathTypePickerViewModel();
            PickerVM.MathTypeSelected += PickerVM_MathTypeSelected;
            MathTypePicker.Model = PickerVM;

            PickerDelegate = new MathTypePickerDelegate();
            PickerDelegate.MathTypeSelected += PickerVM_MathTypeSelected;
            MathTypePicker.Delegate = PickerDelegate;


            if(Vm.PlateMathID > 0)
            {
                MathTypePicker.Select(Vm.PlateMathID, 0, false);
            }
            

            ExerciseNameTextField.EditingChanged += (s, e) => { };

            bindings.Add(this.SetBinding(
                () => Vm.Title,
                () => Title));

            bindings.Add(this.SetBinding(
                () => Vm.NameInputEnabled,
                () => ExerciseNameTextField.Enabled));

            bindings.Add(this.SetBinding(
                () => Vm.SubmitButtonText,
                () => SubmitButtonText));

            bindings.Add(this.SetBinding(
               () => Vm.ExerciseNameInput,
               () => ExerciseNameTextField.Text,
               BindingMode.TwoWay)
               .ObserveTargetEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
              () => Vm.RepCountInput,
              () => RepCountTextField.Text,
              BindingMode.TwoWay)
              .ObserveTargetEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
              () => Vm.WeightIncrementInput,
              () => WeightIncrementTextField.Text,
              BindingMode.TwoWay)
              .ObserveTargetEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => Vm.RestPeriodMinutesInput,
                () => RestPeriodMinutesTextField.Text,
                BindingMode.TwoWay)
                .ObserveTargetEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
                () => Vm.RestPeriodSecondsInput,
                () => RestPeriodSecondsTextField.Text,
                BindingMode.TwoWay)
                .ObserveTargetEvent("EditingChanged"));

            bindings.Add(this.SetBinding(
               () => Vm.ConsecutiveSetsInput,
               () => ConsecutiveSetsTextField.Text,
               BindingMode.TwoWay)
               .ObserveTargetEvent("EditingChanged"));

            ExerciseNameTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RepCountTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            WeightIncrementTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RestPeriodMinutesTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RestPeriodSecondsTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            ConsecutiveSetsTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            RepCountTextField.AddDoneButtonToNumericKeyboard();
            WeightIncrementTextField.AddDoneButtonToNumericKeyboard();
            RestPeriodMinutesTextField.AddDoneButtonToNumericKeyboard();
            RestPeriodSecondsTextField.AddDoneButtonToNumericKeyboard();
            ConsecutiveSetsTextField.AddDoneButtonToNumericKeyboard();

            RestPeriodSecondsTextField.EditingDidEnd += RestPeriodSecondsTextField_EditingDidEnd;
            RestPeriodMinutesTextField.EditingDidEnd += RestPeriodMinutesTextField_EditingDidEnd;
            Vm.InfoUser();
        }

        private void RestPeriodMinutesTextField_EditingDidEnd(object sender, EventArgs e)
        {
            Vm.NormalizeRestPeriodMinutes();
        }

        private void RestPeriodSecondsTextField_EditingDidEnd(object sender, EventArgs e)
        {
            Vm.NormalizeRestPeriodSeconds();
        }

        private void PickerVM_MathTypeSelected(IPlateMath obj)
        {
            System.Diagnostics.Debug.WriteLine("PickerVM_MathTypeSelected");
            Console.WriteLine("PickerVM_MathTypeSelected");
            Vm.PlateMath = obj;
        }

        class MathTypePickerDelegate : UIPickerViewDelegate
        {
            public event Action<IPlateMath> MathTypeSelected;

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

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                System.Diagnostics.Debug.WriteLine("Selected in MathTypePickerDelegate");
                Console.WriteLine("Selected in MathTypePickerDelegate");
                PlateMath pm = PlateMath.PlateMathTypes[row];
                MathTypeSelected?.Invoke(pm);
            }
        }

        class MathTypePickerViewModel : UIPickerViewModel
        {
            public event Action<IPlateMath> MathTypeSelected;

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

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                System.Diagnostics.Debug.WriteLine("Selected");
                Console.WriteLine("Selected");
                PlateMath pm = PlateMath.PlateMathTypes[row];
                MathTypeSelected?.Invoke(pm);
            }
        }
    }
}