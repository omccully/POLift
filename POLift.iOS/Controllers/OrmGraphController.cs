using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

using OxyPlot.Xamarin.iOS;
using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;
using CoreGraphics;

namespace POLift.iOS.Controllers
{
    public partial class OrmGraphController : UIViewController
    {
        // Keep track of bindings to avoid premature garbage collection
        private readonly List<Binding> bindings = new List<Binding>();

        private OrmGraphViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.OrmGraph;
            }
        }

        public OrmGraphController (IntPtr handle) : base (handle)
        {
        }

        PlotView plot_view;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            plot_view = new PlotView();

            CGRect frame = this.View.Frame;
            frame.Y += 10;
            frame.Height -= 10;

            plot_view.Frame = frame;

            bindings.Add(this.SetBinding(
                () => Vm.PlotModel,
                () => plot_view.Model));

            this.View.AddSubview(plot_view);

            Vm.PromptUser();
        }
    }
}