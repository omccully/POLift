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
        Func<Bundle> GetTimerState;
        Intent parent_intent;

        NotificationManager mNotificationManager;

        public NotificationService(Context context, Intent parent_intent, Func<Bundle> GetTimerState)
        {
            this.context = context;
            this.parent_intent = parent_intent;
            this.GetTimerState = GetTimerState;

            mNotificationManager =
                (NotificationManager)context.GetSystemService(
                    Context.NotificationService);
        }

        public void Notify()
        {
            Intent result_intent = new Intent(context, this.GetType());

            result_intent.PutExtras(GetTimerState());

            TaskStackBuilder tsb = TaskStackBuilder.Create(context)
               //.AddParentStack(this.GetType())
               //.AddParentStack(this)
               .AddParentStack(Java.Lang.Class.FromType(context.GetType()))
               .AddNextIntent(result_intent);

            Intent new_pi = tsb.EditIntentAt(tsb.IntentCount - 2);
            System.Diagnostics.Debug.WriteLine("IntentAt(0) is " + tsb.EditIntentAt(0).Component.ClassName);
            System.Diagnostics.Debug.WriteLine("IntentAt(1) is " + tsb.EditIntentAt(1).Component.ClassName);
            System.Diagnostics.Debug.WriteLine("parent is " + new_pi.Component.ClassName);
            if (parent_intent != null) new_pi.PutExtras(parent_intent);

            //new_pi.PutExtra("backed_into", true);

            new_pi.PutExtra("resume_routine_result_id", 0);

            System.Diagnostics.Debug.WriteLine(tsb.ToString());
            System.Diagnostics.Debug.WriteLine("count = " + tsb.ToEnumerable<Intent>().Count());

            PendingIntent resultPendingIntent = tsb
                //.AddNextIntentWithParentStack(result_intent)
                //.GetPendingIntent();
                .GetPendingIntent(500, (int)PendingIntentFlags.UpdateCurrent);
            // PendingIntent.

            //resultPendingIntent
            NotificationCompat.Builder n_builder = new NotificationCompat.Builder(context)
               //.SetSmallIcon(Resource.Drawable.timer_white)
               .SetSmallIcon(Resource.Drawable.abc_ab_share_pack_mtrl_alpha)
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