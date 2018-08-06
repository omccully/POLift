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
using Google.MobileAds;
using POLift.iOS.Service;

namespace POLift.iOS.Controllers
{
    public partial class PerformRoutineController : PerformRoutineBaseController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        protected override PerformBaseViewModel BaseVm => Vm;

        private PerformRoutineViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.PerformRoutine;
            }
        }

        public PerformRoutineController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Console.WriteLine("PerformRoutineController");

            WeightTextField.ShouldReturn = AppleHelpers.DismissKeyboard;
            RepCountTextField.ShouldReturn = AppleHelpers.DismissKeyboard;

            WeightTextField.AddDoneButtonToNumericKeyboard();
            RepCountTextField.AddSubmitCancelButtonsToNumericKeyboard(delegate
            {
                System.Diagnostics.Debug.WriteLine("ENTER");
                Vm.SubmitResultFromInput();
            });

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

            WeightTextField.EditingChanged += (s, e) => { };
            WeightTextField.ValueChanged += (s, e) => { };
          
            WeightTextField.Text = "";
            bindings.Add(this.SetBinding(
                () => WeightTextField.Text,
                () => Vm.WeightInputText,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged"));
            
            RepCountTextField.EditingChanged += (s, e) => { };
            RepCountTextField.ValueChanged += (s, e) => { };
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

            Vm.ResetWeightInput();
            Vm.PromptUser();

            Vm.ResultSubmittedWithoutCompleting += Vm_ResultSubmittedWithoutCompleting;

            Console.WriteLine("view load finished");
        }

        protected void ResultSubmittedWithoutCompleting(object sender, EventArgs e)
        {
            bool asked_for_rating = Vm.PromptUserForRating(delegate
            {
                AppleHelpers.OpenRateApp();
            });

            if(!asked_for_rating)
            {
                Vm.PromptUserForFeedback(delegate
                {
                    AppleHelpers.OpenHelpAndFeedback();
                });
            }
        }

        public override void ViewDidUnload()
        {
            Vm.ResultSubmittedWithoutCompleting -= Vm_ResultSubmittedWithoutCompleting;

            base.ViewDidUnload();
        }

        private void Vm_ResultSubmittedWithoutCompleting(object sender, EventArgs e)
        {
            Vm.PromptUserForRating(AppleHelpers.OpenRateApp);
        }
    }
}