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

    static class Helpers
    {
        public static void DisplayError(Context context, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(context);
            dialog.SetMessage(message);
            dialog.SetNeutralButton("Ok", delegate { });
            dialog.Show();
        }

        public static void DisplayConfirmation(Context context, string message, 
            EventHandler<DialogClickEventArgs> action_if_yes,
            EventHandler<DialogClickEventArgs> action_if_no = null)
        {
            if (action_if_no == null) action_if_no = delegate { };
            AlertDialog.Builder dialog = new AlertDialog.Builder(context);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("Yes", action_if_yes);
            dialog.SetNegativeButton("No", action_if_no);
            dialog.Show();
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

        public static int GetClosestToIncrement(int value, int increment, bool prefer_up = false)
        {
            int floor_increments = value / increment;

            int floor = floor_increments * increment;
            int ceil = floor + increment;

            int floor_dif = value - floor;
            int ceil_dif = ceil - value;

            if (floor_dif == ceil_dif)
            {
                return (prefer_up ? ceil : floor);
            }
            else
            {
                return floor_dif > ceil_dif ? ceil : floor;
            }
        }
    }
}