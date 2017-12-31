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

        public override UIViewController[] ViewControllers { get => base.ViewControllers;
            set
            {
                base.ViewControllers = value;
                Console.WriteLine("ViewControllers.Set");
            }
                
               

        }
        public override void PushViewController(UIViewController viewController, bool animated)
        {
            Console.WriteLine("before push, ViewControllers.Length=" + ViewControllers.Length);
            for(int i = 0; i < ViewControllers.Length; i++)
            {
                Console.WriteLine(i + ". " + ViewControllers[i].GetType().FullName);
            }

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
                viewController.NavigationItem.SetLeftBarButtonItem(
                    new UIBarButtonItem("nav",
                    UIBarButtonItemStyle.Plain,
                    delegate
                    {
                        NavigationMenuButtonClicked?.Invoke(viewController, new EventArgs());
                    }), true);
            }
            

            base.PushViewController(viewController, animated);


            // Console.WriteLine("PushViewController, this.TopViewController= " + this.TopViewController);
        }


    }
}