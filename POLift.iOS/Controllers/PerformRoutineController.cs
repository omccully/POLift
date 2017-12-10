﻿using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using POLift.Core.Model;
using POLift.Core.Service;
using POLift.Core.Helpers;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;

using Unity;

namespace POLift.iOS.Controllers
{
    public partial class PerformRoutineController : DatabaseController, IValueReturner<IRoutineResult>
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        public event Action<IRoutineResult> ValueChosen;

        private PerformRoutineViewModel Vm
        {
            get
            {
                return Application.Locator.PerformRoutine;
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

            ReportResultButton.SetCommand(
                "TouchUpInside",
                Vm.SubmitResultCommand);

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



            //Vm.Routine = 
            //    C.ontainer.Resolve<IPOLDatabase>()
            //    .ReadByID<Routine>(1);

            Vm.ResetWeightInput();

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