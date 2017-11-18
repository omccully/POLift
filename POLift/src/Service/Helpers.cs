using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

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

        public static long UnixTimeStamp()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static int OneRepMax(float weight, int reps)
        {
            int rep_index = reps - 1;

            if (reps < 1)
            {
                throw new ArgumentOutOfRangeException("Reps is not in range");
            }

            int percent;
            if (rep_index >= PercentOf1RepMaxFromReps.Length)
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
            EventHandler<DialogClickEventArgs> action_when_ok = null)
        {
            if (action_when_ok != null) action_when_ok = delegate { };

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

        public static string ToIDString(this IEnumerable<int> ids)
        {
            return String.Join(",", ids.ToArray());
        }

        public static int[] ToIDIntegers(this string str)
        {
            if (String.IsNullOrEmpty(str)) return new int[] { };
            return str.Split(',').Select(txt => Int32.Parse(txt)).ToArray();
        }

        public static float GetClosestToIncrement(float value, float increment,
            float offset = 0, bool prefer_up = false)
        {
            value -= offset;

            int floor_increments = (int)(value / increment);

            float floor = floor_increments * increment;
            float ceil = floor + increment;

            float floor_dif = value - floor;
            float ceil_dif = ceil - value;

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

            foreach (Tuple<int, string> time_period in TimePeriods)
            {
                if (ts.TotalSeconds >= time_period.Item1)
                {
                    int days = (int)ts.TotalSeconds / time_period.Item1;
                    string result = $"{days} {time_period.Item2}";

                    if (days > 1)
                    {
                        result += "s";
                    }

                    return result;
                }
            }

            return "0 seconds";
        }

        public static string Plur(int num)
        {
            return num == 1 ? "" : "s";
        }

        public static async Task<string> HttpQueryAsync(string url)
        {
            WebRequest web_request = HttpWebRequest.Create(url);
            web_request.Timeout = 4000;
            web_request.Proxy = null;

            Log.Info("POLift", "Querying server " + url);
            Task<WebResponse> response_task = web_request.GetResponseAsync();

            // force a 4000 ms timeout because something weird was happening
            if (await Task.WhenAny(response_task, Task.Delay(5000)) != response_task)
            {
                // timeout
                System.Diagnostics.Debug.WriteLine("Timeout " + url);
                throw new TimeoutException();
            }
            System.Diagnostics.Debug.WriteLine("Received response from server  " + url);

            WebResponse web_response = await response_task;

            using (Stream response_stream = web_response.GetResponseStream())
            {
                using (StreamReader reponse_reader = new StreamReader(response_stream))
                {
                    return reponse_reader.ReadToEnd();
                }
            }
        }

        public static Stream HttpQueryStream(string url)
        {
            WebRequest web_request = HttpWebRequest.Create(url);
            web_request.Timeout = 4000;
            web_request.Proxy = null;

            Log.Info("POLift", "Querying server " + url);
            WebResponse web_response = web_request.GetResponse();

            System.Diagnostics.Debug.WriteLine("Received response from server  " + url);

            return web_response.GetResponseStream();
        }

        public static string HttpQuery(string url)
        {
            using (Stream response_stream = HttpQueryStream(url))
            {
                using (StreamReader reponse_reader = new StreamReader(response_stream))
                {
                    return reponse_reader.ReadToEnd();
                }
            }
        }

        public static void WriteStreamToFile(Stream read_stream, string file_path)
        {
            using (FileStream file_write_stream = File.Create(file_path))
            {
                read_stream.CopyTo(file_write_stream);
            }
        }

        public static void ImportFromUri(Android.Net.Uri uri, IPOLDatabase Database, ContentResolver content_resolver, 
            string temp_dir, bool full = true)
        {
            const string ImportFile = "database-import.db3";
            string ImportFilePath = Path.Combine(temp_dir, ImportFile);

            WriteStreamToFile(content_resolver.OpenInputStream(uri), ImportFilePath);

            ImportDatabaseFromLocalFile(ImportFilePath, Database, full);

            try
            {
                File.Delete(ImportFilePath);
            }
            catch { }
        }

        public static void ImportFromUrl(string url, IPOLDatabase Database, string temp_dir, bool full=true)
        {
            const string ImportFile = "database-import.db3";
            string ImportFilePath = Path.Combine(temp_dir, ImportFile);

            using (Stream response_stream = HttpQueryStream(url))
            {
                WriteStreamToFile(response_stream, ImportFilePath);
            }

            ImportDatabaseFromLocalFile(ImportFilePath, Database, full);

            try
            {
                File.Delete(ImportFilePath);
            }
            catch { }
        }

        public static void ImportDatabaseFromLocalFile(string local_file, 
            IPOLDatabase Database, bool full=true)
        {
            POLDatabase imported = new POLDatabase(local_file);

            if (full)
            {
                Database.ImportDatabase(imported);
            }
            else
            {
                Database.ImportRoutinesAndExercises(imported);
            }
        }
    }
}