using System;
using System.Drawing;

using CoreFoundation;
using UIKit;
using Foundation;
using POLift.Core.Service;

using GalaSoft.MvvmLight.Views;

namespace POLift.iOS.Controllers
{
    public class DatabaseController : UIViewController, IDatabaseUser
    {
        public IPOLDatabase Database { get; set; }

        public DatabaseController(IntPtr handle) : base(handle)
        {
        }

        public DatabaseController()
        {
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            var next_controller = segue.DestinationViewController as IDatabaseUser;
           // Console.WriteLine("Preparing ");
            if(next_controller != null)
            {
               // Console.WriteLine(this.GetType().ToString() + " to " + next_controller.GetType());
                next_controller.Database = Database;
            }
        }
    }
}