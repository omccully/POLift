using Foundation;
using System;
using UIKit;
using System.Linq;

using System.Collections.Generic;

using SidebarNavigation;

namespace POLift.iOS.Controllers
{
    public partial class NavigationController : UINavigationController
    {
        //SidebarController sidebar_controller;
        public event EventHandler NavigationMenuButtonClicked;

        public NavigationController (IntPtr handle) : base (handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //sidebar_controller = new SidebarController(this.ViewController)

            //Console.WriteLine("ViewDidLoad, this.TopViewController= " + this.TopViewController);
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {

           /* Console.WriteLine("before push, ViewControllers.Length=" + ViewControllers.Length);
            for(int i = 0; i < ViewControllers.Length; i++)
            {
                Console.WriteLine(i + ". " + ViewControllers[i].GetType().FullName);
            }*/

            Type vc_type = viewController.GetType();

            int last_index_of_type = -1;
            for (int i = ViewControllers.Length - 1; i >= 0; i--)
            {
                if(ViewControllers[i].GetType() == vc_type)
                {
                    last_index_of_type = i;
                    break;
                }
            }


            if (last_index_of_type != -1)
            {
                UIViewController[] conts = ViewControllers
                    .Skip(last_index_of_type + 1).ToArray();

                SetViewControllers(conts, false);
            }
            //new UIBarButtonItem()
            if (ViewControllers.Length == 0)
            {
                UIImage image = UIImage.FromBundle("NavigationIcon");

                EventHandler action = delegate
                {
                    NavigationMenuButtonClicked?.Invoke(viewController, new EventArgs());
                };

                UIButton button = new UIButton(UIButtonType.Custom);
                button.SetImage(image, UIControlState.Normal);
                button.AddTarget(action, UIControlEvent.TouchUpInside);
                button.Frame = new CoreGraphics.CGRect(0, 0, 30, 30);

                UIBarButtonItem bar_button = new UIBarButtonItem(button);

                /* new UIBarButtonItem("nava",
                    UIBarButtonItemStyle.Plain,
                    delegate
                    {
                        NavigationMenuButtonClicked?.Invoke(viewController, new EventArgs());
                    })*/

                viewController.NavigationItem.SetLeftBarButtonItem(bar_button, true);
            }
            

            base.PushViewController(viewController, animated);


            // Console.WriteLine("PushViewController, this.TopViewController= " + this.TopViewController);
        }


    }
}
 