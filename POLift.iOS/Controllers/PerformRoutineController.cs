using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using POLift.Core.Model;
using POLift.Core.Service;
using POLift.Core.Helpers;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class PerformRoutineController : UIViewController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private PerformRoutineViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.PerformRoutine;
            }
        }

        private TimerViewModel TimerVm
        {
            get
            {
                return ViewModelLocator.Default.Timer;
            }
        }

        public PerformRoutineController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Console.WriteLine("PerformRoutineController");

            WeightTextField.EditingChanged += WeightTextField_ValueChanged;
            WeightTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RepCountTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            WeightTextField.AddDoneButtonToNumericKeyboard();
            RepCountTextField.AddDoneButtonToNumericKeyboard();

            ReportResultButton.SetCommand(
                "TouchUpInside",
                Vm.SubmitResultCommand);

            ModifyRestOfRoutineButton.SetCommand(
                "TouchUpInside",
                Vm.ModifyRestOfRoutineCommand);

            IMadeAMistakeButton.SetCommand(
                "TouchUpInside",
                Vm.IMadeAMistakeCommand);

            EditCurrentExerciseButton.SetCommand(
                "TouchUpInside",
                Vm.EditThisExerciseCommand);

            //WeightTextField.SetCommand(
            //  "ValueChanged",
            //  Vm.WeightInputChangedCommand);

            WeightTextField.EditingChanged += (s, e) => { };
            WeightTextField.ValueChanged += (s, e) => { };
            //this.SetBinding(
            //   () => WeightTextField.Text)
            //    .UpdateSourceTrigger("ValueChanged")
            //    .WhenSourceChanges(() => Vm.WeightInputText = WeightTextField.Text);
            WeightTextField.Text = "";
            bindings.Add(this.SetBinding(
                () => WeightTextField.Text,
                () => Vm.WeightInputText,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));
            // .WhenSourceChanges(() => Vm.WeightInputText = WeightTextField.Text);

            
            RepCountTextField.EditingChanged += (s, e) => { };
            RepCountTextField.ValueChanged += (s, e) => { };
            RepCountTextField.EditingChanged += RepCountTextField_ValueChanged;
            RepCountTextField.Text = "";
            bindings.Add(this.SetBinding(
                () => RepCountTextField.Text,
                () => Vm.RepsInputText,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));

            bindings.Add(
                this.SetBinding(
                    () => Vm.RoutineDetails,
                    () => RoutineDetailsLabel.Text));

            bindings.Add(
               this.SetBinding(
                   () => Vm.ExerciseDetails,
                   () => ExerciseDetailsLabel.Text));

            bindings.Add(
               this.SetBinding(
                   () => Vm.PlateMathDetails,
                   () => PlateMathLabel.Text));

            bindings.Add(
               this.SetBinding(
                   () => Vm.RepDetails,
                   () => RepDetailsLabel.Text));


            bool t = TimerVm.TimerIsStartable;
            bindings.Add(
                this.SetBinding(
                    () => TimerVm.TimerIsStartable,
                    () => ReportResultButton.Enabled));

            bindings.Add(
                this.SetBinding(
                    () => TimerVm.TimerIsStartable,
                    () => RepCountTextField.Enabled));


            //Vm.Routine = 
            //    C.ontainer.Resolve<IPOLDatabase>()
            //    .ReadByID<Routine>(1);

            Vm.ResetWeightInput();
            Vm.PromptUser();

            Console.WriteLine("view load finished");

        }

        private void RepCountTextField_ValueChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("RepCountTextField_EditingChanged to " + RepCountTextField.Text);
        }

        private void WeightTextField_ValueChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("WeightTextField_EditingChanged" + WeightTextField.Text);
        }



        
    }
}