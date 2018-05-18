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
            options.Add(Constants.kCRToastTextMaxNumberOfLinesKey, message.Count(c => c == '\n') + 1);

            options.Add(Constants.kCRToastAnimationInTypeKey, CRToastAnimationType.Gravity);
            options.Add(Constants.kCRToastAnimationOutTypeKey, CRToastAnimationType.Gravity);
            options.Add(Constants.kCRToastAnimationInDirectionKey, CRToastAnimationDirection.Top);
            options.Add(Constants.kCRToastAnimationOutDirectionKey, CRToastAnimationDirection.Top);

            options.Add(Constants.kCRToastTextAlignmentKey, CRToastAccessoryViewAlignment.Center);

            // have it depend on message.Length
            int hold_time = message.Length / 15;
            hold_time = Math.Min(hold_time, 5);
            hold_time = Math.Max(hold_time, 2);

            options.Add(Constants.kCRToastTimeIntervalKey, hold_time);

            options.Add(Constants.kCRToastUnderStatusBarKey, true);
            options.Add(Constants.kCRToastNotificationTypeKey, CRToast.CRToastType.NavigationBar);
 
            NSDictionary dict = NSDictionary.FromObjectsAndKeys(
                options.Values.ToArray(),
                options.Keys.ToArray());
            
            CRToastManager.ShowNotificationWithOptions(dict, delegate { });
        }
    }
}
