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

        }
    }
}