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
    public class DialogMessageService : IDialogMessageService
    {
        public void DisplayAcknowledgement(string message, Action action_when_ok = null)
        {
            UIAlertView alert = new UIAlertView()
            {
                Message = message
            };

            alert.AddButton("OK");
            
            if(action_when_ok != null)
            {
                alert.Clicked += delegate (object sender, UIButtonEventArgs e)
                {
                    System.Diagnostics.Debug.WriteLine("buttonindex = " + e.ButtonIndex);
                    if (e.ButtonIndex == 0)
                    {
                        action_when_ok.Invoke();
                    }
                };
            }

            alert.Show();
        }

        public void DisplayConfirmation(string message, Action action_if_yes, Action action_if_no = null)
        {
            UIAlertView alert = new UIAlertView()
            {
                Message = message
            };

            alert.AddButton("Yes");
            alert.AddButton("No");
            
            alert.Clicked += delegate (object sender, UIButtonEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("buttonindex = " + e.ButtonIndex);
                if (e.ButtonIndex == 0)
                {
                    action_if_yes?.Invoke();
                }
                else
                {
                    action_if_no?.Invoke();
                }
            };

            alert.Show();

        }

        public void DisplayConfirmationNeverShowAgain(string message, string preference_key, Action action_if_yes, Action action_if_no = null)
        {
            DisplayConfirmation(message, action_if_yes, action_if_no);

            //throw new NotImplementedException();
        }

        public void DisplayConfirmationYesNotNowNever(string message, string ask_for_key, Action action_if_yes)
        {
            DisplayConfirmation(message, action_if_yes);
            //throw new NotImplementedException();
        }

        public void DisplayTemporaryError(string message)
        {
            MakeToast(message, UIColor.Red);
        }

        public void DisplayTemporaryMessage(string message)
        {
            MakeToast(message, UIColor.Black);
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