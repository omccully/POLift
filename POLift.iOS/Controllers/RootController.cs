using Foundation;
using System;
using UIKit;
using SidebarNavigation;

namespace POLift.iOS.Controllers
{
    public partial class RootController : UIViewController
    {
        // the sidebar controller for the app
        public SidebarController SidebarController { get; private set; }
        public NavigationController NavController { get; private set; }

        public RootController(IntPtr intPtr) : base(intPtr)
        {
            NavController = (NavigationController)this.Storyboard
                .InstantiateViewController("Navigator");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            MainController main = (MainController)this.Storyboard
                .InstantiateViewController("MainPage");
            SideMenuController side_menu = (SideMenuController)
                this.Storyboard.InstantiateViewController("SideMenuPage");

            NavController.NavigationMenuButtonClicked += NavController_NavigationMenuButtonClicked;
            NavController.PushViewController(main, false);
            
            SidebarController = new LightSidebarController(this, NavController,
                side_menu);

            //SidebarController.StatusBar
            //SidebarController.PreferredStatusBarStyle = UIStatusBarStyle.LightContent;
            side_menu.SidebarController = SidebarController;
            
            SidebarController.MenuWidth = 280;
            SidebarController.ReopenOnRotate = false;
            SidebarController.MenuLocation = MenuLocations.Left;

            if(this.View.GestureRecognizers != null)
            {
                Console.WriteLine("RootController");
                foreach (UIGestureRecognizer rec in this.View.GestureRecognizers)
                {
                    Console.WriteLine("UIGestureRecognizer " + rec.DebugDescription);
                    rec.CancelsTouchesInView = false;
                }
            }
            
        }

        private void NavController_NavigationMenuButtonClicked(object sender, EventArgs e)
        {
            SidebarController.ToggleMenu();
        }
    }
}