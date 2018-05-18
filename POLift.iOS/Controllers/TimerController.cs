using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

using POLift.Core.ViewModel;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Helpers;

using System.Threading;

using POLift.iOS.Service;

namespace POLift.iOS.Controllers
{
    public partial class TimerController : UIViewController
    {
        private readonly List<Binding> bindings = new List<Binding>();

        private TimerViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.Timer;
            }
        }

        public TimerController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Console.WriteLine("Starting on thread " + Thread.CurrentThread.ManagedThreadId); 

            SkipTimerButton.Enabled = true;
            SkipTimerButton.SetCommand(Vm.SkipTimerCommand);
            bindings.Add(this.SetBinding(
                () => Vm.SkipTimerEnabled,
                () => SkipTimerButton.Enabled));

            Sub30SecButton.SetCommand(Vm.Sub30SecCommand);
            bindings.Add(this.SetBinding(
                () => Vm.Sub30SecEnabled,
                () => Sub30SecButton.Enabled));

            Add30SecButton.SetCommand(Vm.Add30SecCommand);
            bindings.Add(this.SetBinding(
                () => Vm.Add30SecEnabled,
                () => Add30SecButton.Enabled));

            bindings.Add(this.SetBinding(
               () => Vm.TimerStatus,
               () => TimerStatusLabel.Text));

            bindings.Add(this.SetBinding(
               () => Vm.TimerState,
               () => this.TimerState));


            // button text
            bindings.Add(this.SetBinding(
               () => Vm.Add30SecButtonText,
               () => Add30SecButtonText));

            bindings.Add(this.SetBinding(
               () => Vm.Sub30SecButtonText,
               () => Sub30SecButtonText));

        }

        public string Add30SecButtonText
        {
            get
            {
                return Add30SecButton.Title(UIControlState.Normal);
            }
            set
            {
                Add30SecButton.SetTitle(value, UIControlState.Normal);
            }
        }

        public string Sub30SecButtonText
        {
            get
            {
                return Sub30SecButton.Title(UIControlState.Normal);
            }
            set
            {
                Sub30SecButton.SetTitle(value, UIControlState.Normal);
            }
        }

        TimerState _TimerState = TimerState.Skipped;
        public TimerState TimerState
        {
            get
            {
                return _TimerState;
            }
            set
            {
                switch (value)
                {
                    case TimerState.Skipped:
                        TimerStatusLabel.TextColor = UIColor.Black;
                        break;
                    case TimerState.RunningPositive:
                        // dark orange
                        TimerStatusLabel.TextColor = UIColor.FromRGBA(255, 140, 0, 255);
                        break;
                    case TimerState.Elapsed:
                        // dark green
                        TimerStatusLabel.TextColor = UIColor.FromRGBA(0, 100, 0, 255);
                        break;
                }

                _TimerState = value;
            }
        }
    }
}