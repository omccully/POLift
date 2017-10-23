using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        static int[] PercentOf1RepMaxFromReps = 
        {
            100, 95, 93, 90, 87,
            85, 83, 80, 77, 75,
            73, 70, 66, 63, 60
        };

        public static int OneRepMax(int weight, int reps)
        {
            int rep_index = reps - 1;

            if(reps < 1)
            {
                throw new ArgumentOutOfRangeException("Reps is not in range");
            }

            int percent;
            if(rep_index >= PercentOf1RepMaxFromReps.Length)
            {
                percent = PercentOf1RepMaxFromReps.Last();
            }
            else
            {
                percent = PercentOf1RepMaxFromReps[rep_index];
            }

            return (int)(weight * (100.0 / percent));
        }

        public static int RemoveAll<T>(this ObservableCollection<T> collection,
                                                       Func<T, bool> condition)
        {
            int removed_count = 0;
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                    removed_count++;
                }
            }
            return removed_count;
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

        public static int[] ToIDIntegers(this string str)
        {
            if (String.IsNullOrEmpty(str)) return new int[] { };
            return str.Split(',').Select(txt => Int32.Parse(txt)).ToArray();
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

        public static string TranslateIDString(string old_id_string, Dictionary<int, int> mapping)
        {
            return String.Join(",", old_id_string.ToIDIntegers().Select(old =>
                mapping.ContainsKey(old) ? mapping[old] : old)
            .ToArray());
        }

        public static string ToRoundedString(this TimeSpan ts)
        {
            const int SecondsInMinute = 60;
            const int SecondsInHour = 60 * SecondsInMinute;
            const int SecondsInDay = 24 * SecondsInHour;

            List<Tuple<int, string>> TimePeriods = new List<Tuple<int, string>>();
            TimePeriods.Add(new Tuple<int, string>(SecondsInDay, "day"));
            TimePeriods.Add(new Tuple<int, string>(SecondsInHour, "hour"));
            TimePeriods.Add(new Tuple<int, string>(SecondsInMinute, "minute"));
            TimePeriods.Add(new Tuple<int, string>(1, "second"));

            foreach(Tuple<int,string> time_period in TimePeriods)
            {
                if (ts.TotalSeconds >= time_period.Item1)
                {
                    int days = (int)ts.TotalSeconds / time_period.Item1;
                    string result = $"{days} {time_period.Item2}";

                    if(days > 1)
                    {
                        result += "s";
                    }

                    return result;
                }
            }

            return "0 seconds"; 
        }
    }
}