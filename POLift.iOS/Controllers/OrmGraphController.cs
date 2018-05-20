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

            int status_bar_height = 64;
            CGRect frame = this.View.Frame;
            frame.Y += status_bar_height;
            frame.Height -= status_bar_height;
            frame.Width -= 10;
            plot_view.Frame = frame;

            plot_view.Model = Vm.PlotModel;
            System.Diagnostics.Debug.WriteLine("fontsize = " + plot_view.Model.TitleFontSize);
            plot_view.Model.TitleFontSize = 16.0;
            //bindings.Add(this.SetBinding(
            //    () => Vm.PlotModel,
            //    () => plot_view.Model));

            this.View.AddSubview(plot_view);

            plot_view_created = true;

            //Share();
            float bottom = (float)(frame.Y + frame.Height);
            float right = (float)(frame.X + frame.Width);

            int button_width = 60;
            int button_height = 40;

            int button_margin_bottom = 30;
            int button_margin_right = 25;

            float button_x = right - (button_width + button_margin_right);
            float button_y = bottom - (button_height + button_margin_bottom);

            UIButton share_button = new UIButton(UIButtonType.System);
            share_button.Frame = new CGRect(button_x, button_y, button_width, button_height);
            share_button.SetTitle("Share", UIControlState.Normal);
            share_button.TouchUpInside += Share_button_TouchUpInside;


            //UISwitch 

            this.View.AddSubview(share_button);
        }

        private void Share_button_TouchUpInside(object sender, EventArgs e)
        {
            Share();
        }

        void Share()
        {
            if (plot_view == null) return;

            UIGraphics.BeginImageContextWithOptions(plot_view.Bounds.Size, false, new nfloat(0));
            plot_view.Layer.RenderInContext(UIGraphics.GetCurrentContext());

            // draw link
            UIFont font = UIFont.PreferredBody;
            
            CGPoint point = new CGPoint(42, 42);
            NSString str = new NSString("polift-app.com");

            /*CGSize size = str.DrawString(point, font);
            var blue = new CGColor((nfloat)0, (nfloat)0, (nfloat)255);
            var blues = new CGColor[] { blue };
            var grad = new CGGradient(CGColorSpace.CreateGenericRgb(), blues);
            var line_point = new CGPoint(point.X, point.Y + font.LineHeight);
            var line_point_end = new CGPoint(line_point.X, line_point.Y + size.Width);
            UIGraphics.GetCurrentContext().DrawLinearGradient(grad, line_point, line_point_end, CGGradientDrawingOptions.None);
            */

            CGSize size = str.DrawString(point, font);
            System.Diagnostics.Debug.WriteLine("drew at size " + size.ToString());

            UIImage img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            //UIGraphics.Draw
            //img.Draw()

            UIActivityViewController avc = new UIActivityViewController(
                new NSObject[] { img }, null);

            PresentViewController(avc, false, null);
        }
    }
}