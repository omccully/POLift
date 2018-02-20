using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class PerformWarmupController : UIViewController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private PerformWarmupViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.PerformWarmup;
            }
        }

        private TimerViewModel TimerVm
        {
            get
            {
                return ViewModelLocator.Default.Timer;
            }
        }

        public PerformWarmupController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetCompletedButton.SetCommand(
                "TouchUpInside",
                Vm.SetCompletedCommand);

            SkipWarmupButton.SetCommand(
                "TouchUpInside",
                Vm.SkipWarmupCommand);

            WeightTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            WeightTextField.EditingChanged += (s, e) => { };
            
            bindings.Add(this.SetBinding(
                () => Vm.WeightInputText,
                () => WeightTextField.Text,
                BindingMode.TwoWay)
                .ObserveTargetEvent("EditingChanged"));

            /*bindings.Add(this.SetBinding(
                () => WeightTextField.Text,
                () => Vm.WeightInputText,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));*/

            bindings.Add(
                this.SetBinding(
                    () => Vm.RoutineDetails,
                    () => RoutineDetailsLabel.Text));

            bindings.Add(
               this.SetBinding(
                   () => Vm.ExerciseDetails,
                   () => ExerciseDetailsLabel.Text));

            bool t = TimerVm.TimerIsStartable;
            bindings.Add(
                this.SetBinding(
                    () => TimerVm.TimerIsStartable,
                    () => SetCompletedButton.Enabled));
        }
    }
}