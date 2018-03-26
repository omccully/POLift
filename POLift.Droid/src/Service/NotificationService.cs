using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.Compat;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

using POLift.Core.Service;

namespace POLift.Droid.Service
{
    class NotificationService : INotificationService
    {
        public int NotificationID = 500;
        Context context;
        Func<Bundle> GetActivityState;
        Intent parent_intent;

        NotificationManager mNotificationManager;

        public NotificationService(Context context, Intent parent_intent, 
            Func<Bundle> GetActivityState)
        {
            this.context = context;
            this.parent_intent = parent_intent;
            this.GetActivityState = GetActivityState;

            mNotificationManager =
                (NotificationManager)context.GetSystemService(
                    Context.NotificationService);
        }

        public void Notify()
        {
            System.Diagnostics.Debug.WriteLine("NotificationService.Notify()");
            Intent result_intent = new Intent(context, context.GetType());

            result_intent.PutExtras(GetActivityState());

            TaskStackBuilder tsb = TaskStackBuilder.Create(context)
               .AddParentStack(Java.Lang.Class.FromType(context.GetType()))
               .AddNextIntent(result_intent);

            Intent new_pi = tsb.EditIntentAt(tsb.IntentCount - 2);
            System.Diagnostics.Debug.WriteLine("IntentAt(0) is " + tsb.EditIntentAt(0).Component.ClassName);
            System.Diagnostics.Debug.WriteLine("IntentAt(1) is " + tsb.EditIntentAt(1).Component.ClassName);

            try
            {
                System.Diagnostics.Debug.WriteLine("IntentAt(2) is " + tsb.EditIntentAt(2).Component.ClassName);
            }
            catch { }

            System.Diagnostics.Debug.WriteLine("parent is " + new_pi.Component.ClassName);

            if (parent_intent != null) new_pi.PutExtras(parent_intent);

            new_pi.PutExtra("warmup_prompted", true);

            System.Diagnostics.Debug.WriteLine(tsb.ToString());
            System.Diagnostics.Debug.WriteLine("count = " + tsb.ToEnumerable<Intent>().Count());

            PendingIntent resultPendingIntent = tsb
                .GetPendingIntent(500, (int)PendingIntentFlags.UpdateCurrent);

            // Resource.Drawable.abc_ab_share_pack_mtrl_alpha
            NotificationCompat.Builder n_builder = new NotificationCompat.Builder(context)
               .SetSmallIcon(Resource.Mipmap.ic_timer_white_24dp)
               .SetContentTitle("Lifting rest period finished")
               .SetContentText("Start your next set whenever you are ready")
               .SetContentIntent(resultPendingIntent);

            mNotificationManager.Notify(NotificationID, n_builder.Build());
        }

        public void Cancel()
        {
            mNotificationManager.Cancel(NotificationID);
        }
    }
}