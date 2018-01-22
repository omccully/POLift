using Foundation;
using System;
using UIKit;
using System.Collections.Generic;

using POLift.Core.ViewModel;
using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS.Controllers
{
    public partial class OrmGraphSideMenuController : UIViewController
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

        public OrmGraphSideMenuController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //DataTextLabel.Text = "init";

            bindings.Add(this.SetBinding(
                () => Vm.DataText,
                () => DataTextLabel.Text));

            Console.WriteLine("loading OrmGraphSideMenu, DataText = " + Vm.DataText);
        }
    }
}