using Foundation;
using System;
using UIKit;
using SidebarNavigation;

namespace POLift.iOS.Controllers
{
    public partial class OrmGraphRootController : UIViewController
    {
        public SidebarController SidebarController { get; private set; }

        public OrmGraphRootController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Console.WriteLine("Loading OrmGraphRoot");
            OrmGraphController orm = (OrmGraphController)
                this.Storyboard.InstantiateViewController("OrmGraphPage");

            OrmGraphSideMenuController side_menu = (OrmGraphSideMenuController)
                this.Storyboard.InstantiateViewController("OrmGraphSideMenuPage");

            SidebarController = new SidebarController(this,
                orm, side_menu);

            SidebarController.MenuWidth = 280;
            SidebarController.ReopenOnRotate = false;
            SidebarController.MenuLocation = MenuLocations.Right;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem("Data",
                UIBarButtonItemStyle.Plain, delegate
                {
                    SidebarController.ToggleMenu();
                });
        }
    }
}