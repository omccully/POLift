using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;
using POLift.Core.ViewModel;
using UserNotifications;

namespace POLift.iOS.Service
{
    class NotificationService : INotificationService
    {

        public NotificationService()
        {
           
        }

        public void Notify()
        {
            UNMutableNotificationContent content = new UNMutableNotificationContent();
            content.Title = TimerViewModel.NotificationText;
            content.Body = TimerViewModel.NotificationSubText;
            content.Badge = 1;

            System.Diagnostics.Debug.WriteLine("sound = " + content.Sound);
            content.Sound = UNNotificationSound.Default;
            

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

            var requestID = "timerNotificationRequest";
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);


            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                if (err != null)
                {
                    // Do something with error...
                    System.Diagnostics.Debug.WriteLine("Notification error: " + err);
                }
            });
        }
    }
}