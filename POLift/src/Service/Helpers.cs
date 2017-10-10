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

namespace POLift.Service
{
    using Model;

    public static class Helpers
    {
        public static int OneRepMax(int weight, int reps)
        {
            return 0;
        }

        public static string UniformString(string name)
        {
            return name.ToLower().Trim();
        }

        public static AlertDialog DisplayError(Context context, string message,
            EventHandler<DialogClickEventArgs> action_when_ok=null)
        {
            if(action_when_ok != null) action_when_ok = delegate { };

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetNeutralButton("Ok", action_when_ok);
            //dialog.Show();

            AlertDialog dialog = builder.Create();
            dialog.Show();
            builder.Dispose();
            return dialog;
        }

        public static AlertDialog DisplayConfirmation(Context context, string message, 
            EventHandler<DialogClickEventArgs> action_if_yes,
            EventHandler<DialogClickEventArgs> action_if_no = null)
        {
            if (action_if_no == null) action_if_no = delegate { };
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetPositiveButton("Yes", action_if_yes);
            builder.SetNegativeButton("No", action_if_no);
            //dialog.Show();
            AlertDialog ad = builder.Create();
            ad.Show();
            builder.Dispose();
            return ad;
        }

        public static T MaxObject<T, U>(this IEnumerable<T> source, Func<T, U> selector)
                where U : IComparable<U>
        {
            if (source == null) throw new ArgumentNullException("source");
            bool first = true;
            T maxObj = default(T);
            U maxKey = default(U);
            foreach (var item in source)
            {
                if (first)
                {
                    maxObj = item;
                    maxKey = selector(maxObj);
                    first = false;
                }
                else
                {
                    U currentKey = selector(item);
                    if (currentKey.CompareTo(maxKey) > 0)
                    {
                        maxKey = currentKey;
                        maxObj = item;
                    }
                }
            }
            if (first) throw new InvalidOperationException("Sequence is empty.");
            return maxObj;
        }

        public static string ToIDString(this IEnumerable<IIdentifiable> obj)
        {
            return String.Join(",", obj.Select(e => e.ID).ToArray());
        }

        public static int GetClosestToIncrement(int value, int increment, int offset = 0, bool prefer_up = false)
        {
            value -= offset;

            int floor_increments = value / increment;

            int floor = floor_increments * increment;
            int ceil = floor + increment;

            int floor_dif = value - floor;
            int ceil_dif = ceil - value;

            if (floor_dif == ceil_dif)
            {
                return (prefer_up ? ceil : floor) + offset;
            }
            else
            {
                return (floor_dif > ceil_dif ? ceil : floor) + offset;
            }
        }
    }
}