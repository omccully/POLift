using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using SidebarNavigation;

namespace POLift.iOS.Controllers
{
    class LightSidebarController : SidebarController
    {
        public LightSidebarController(UIViewController rootViewController,
            UIViewController contentViewController, UIViewController menuViewController) : 
            base(rootViewController, contentViewController, menuViewController)
        {

        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

    }
}