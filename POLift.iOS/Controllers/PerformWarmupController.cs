using Foundation;
using System;
using UIKit;
using System.Collections.Generic;
using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;
using Google.MobileAds;

namespace POLift.iOS.Controllers
{
    public partial class PerformWarmupController : PerformRoutineBaseController
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

        protected override PerformBaseViewModel BaseVm => Vm;

        public PerformWarmupController (IntPtr handle) : base (handle)
        {
        }

        const string TestBannerAdId = "ca-app-pub-3940256099942544/2934735716";
        const string BannerAdId = "ca-app-pub-1015422455885077/4098077945";

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
            WeightTextField.AddDoneButtonToNumericKeyboard();

            bindings.Add(this.SetBinding(
                () => Vm.WeightInputText,
                () => WeightTextField.Text,
                BindingMode.TwoWay)
                .ObserveTargetEvent("EditingChanged"));

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

            if (ShowAds)
            {
                Console.WriteLine("Showing ad");

#if DEBUG
                //AdBanner.AdUnitID = "ca-app-pub-3940256099942544/2934735716";
#else
                //AdBanner.AdUnitID = "ca-app-pub-1015422455885077/4098077945";
#endif
                AdBanner.AdUnitID = BannerAdId;

                AdBanner.RootViewController = this;

                Request req = Request.GetDefaultRequest();
#if DEBUG
                req.TestDevices = new string[]
                {
                    "5763FA36-B1DC-4B5A-8B3F-AD07DD5F988A"
                };
#endif
                AdBanner.LoadRequest(req);
            }
            else
            {
                Console.WriteLine("Not showing ads");
            }
        }
    }
}