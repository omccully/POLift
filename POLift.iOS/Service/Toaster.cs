using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;
using CRToast;

namespace POLift.iOS.Service
{
    public class Toaster : IToaster
    {
        public void DisplayMessage(string message)
        {
            MakeToast(message, UIColor.Black);
        }

        public void DisplayError(string message)
        {
            MakeToast(message, UIColor.Red);
        }

        void MakeToast(string message, UIColor color)
        {
            var options = new Dictionary<NSString, object>();

            options.Add(Constants.kCRToastTextKey, message);
            options.Add(Constants.kCRToastBackgroundColorKey, color);
            //options.Add(Constants.kCRToastTextAlignmentKey, CRToast.CRToastType.NavigationBar);
            //options.Add(Constants.Type, CRToast.CRToastType.NavigationBar);
            options.Add(Constants.kCRToastTextMaxNumberOfLinesKey, 5);

            NSDictionary dict = NSDictionary.FromObjectsAndKeys(
                options.Values.ToArray(),
                options.Keys.ToArray());

            CRToastManager.ShowNotificationWithOptions(dict, delegate { });
        }
    }
}
