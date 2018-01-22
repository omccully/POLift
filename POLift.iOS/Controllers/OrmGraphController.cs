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
            Console.WriteLine("constructing new OrmGraphController");

        }

        PlotView plot_view;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Console.WriteLine("plot_view = " + plot_view);

            

            Vm.PropertyChanged += Vm_PropertyChanged;

            Vm.PromptUser();
        }

        bool plot_view_created = false;
        private void Vm_PropertyChanged(object sender, 
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (plot_view_created) return;
            if (e.PropertyName != "PlotModel") return;

            InitializePlot();
        }

        void InitializePlot()
        {
            plot_view = new PlotView();

            CGRect frame = this.View.Frame;
            frame.Y += 10;
            frame.Height -= 10;
            frame.Width -= 10;
            plot_view.Frame = frame;

            plot_view.Model = Vm.PlotModel;
            //bindings.Add(this.SetBinding(
            //    () => Vm.PlotModel,
            //    () => plot_view.Model));

            this.View.AddSubview(plot_view);

            plot_view_created = true;
        }
    }
}