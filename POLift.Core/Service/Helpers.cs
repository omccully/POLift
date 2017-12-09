using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace POLift.Core.Service
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
            //web_request.Timeout = 4000;
            web_request.Proxy = null;

            System.Diagnostics.Debug.WriteLine("Querying server " + url);
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

        public static async Task<Stream> HttpQueryStreamAsync(string url)
        {
            System.Diagnostics.Debug.WriteLine("querying " + url);
            WebRequest web_request = WebRequest.CreateHttp(url); 

            //web_request.Timeout = 4000;

            //Log.Info("POLift", "Querying server " + url);

            //throw new NotImplementedException();

            return (await web_request.GetResponseAsync()).GetResponseStream();

            //System.Diagnostics.Debug.WriteLine("Received response from server  " + url);

            //return web_response.GetResponseStream();
        }

        /*public static async Task<string> HttpQueryAsync(string url)
        {
            using (Stream response_stream = await HttpQueryStreamAsync(url))
            {
                using (StreamReader reponse_reader = new StreamReader(response_stream))
                {
                    return reponse_reader.ReadToEnd();
                }
            }
        }*/

        public static async Task ImportFromUrlAsync(string url, IPOLDatabase Database, string temp_dir, IFileOperations fops, bool full=true)
        {
            const string ImportFile = "database-import.db3";
            string ImportFilePath = Path.Combine(temp_dir, ImportFile);

            using (Stream response_stream = await HttpQueryStreamAsync(url))
            {
                fops.Write(ImportFilePath, response_stream);
            }

            ImportDatabaseFromLocalFile(ImportFilePath, Database, full);

            try
            {
                fops.Delete(ImportFilePath);
            }
            catch { }
        }

        public static void ImportDatabaseFromLocalFile(string local_file, 
            IPOLDatabase Database, bool full=true)
        {
            POLDatabase imported = new POLDatabase(Database.Platform, local_file);

            if (full)
            {
                Database.ImportDatabase(imported);
            }
            else
            {
                Database.ImportRoutinesAndExercises(imported);
            }
        }

        // Ex: collection.TakeLast(5);
        public static IEnumerable<T> TakeLastEx<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static IEnumerable<IRoutineWithLatestResult> 
            MainPageRoutinesList(IPOLDatabase database)
        {
            List<IRoutineWithLatestResult> result = 
                new List<IRoutineWithLatestResult>();

            foreach(Routine r in database.TableWhereUndeleted<Routine>())
            {
                IRoutineResult latest_rr =
                      RoutineResult.MostRecentForRoutine(database, r);
                result.Add(new RoutineWithLatestResult(r, latest_rr));
            }

            return result.OrderBy(t =>
            {
                if (t.LatestResult == null)
                    return DateTime.MinValue.AddSeconds(1);
                if (!t.LatestResult.Completed)
                    return DateTime.MinValue;

                return t.LatestResult.StartTime;
            });

            /*
            return database.TableWhereUndeleted<Routine>().OrderBy(r =>
            {
                IRoutineResult latest_rr =
                    RoutineResult.MostRecentForRoutine(database, r);
                if (latest_rr == null)
                    return DateTime.MinValue.AddSeconds(1);
                if (!latest_rr.Completed)
                    return DateTime.MinValue;

                return latest_rr.StartTime;
            });*/

        }

        public static void ConvertEverythingToMetric(this IPOLDatabase Database)
        {
            foreach (Exercise ex in Database.Table<Exercise>())
            {
                ex.WeightIncrement /= 2;

                IPlateMath pm = ex.PlateMath;

                if (pm == null)
                {

                }
                else if (pm.BarWeight == 45)
                {
                    ex.PlateMath = PlateMath.MetricBarbellAndPlates;
                }
                else
                {
                    if (pm.BarWeight == 0)
                    {
                        // no bar
                        if (pm.SplitWeights)
                        {
                            ex.PlateMath = PlateMath.MetricPlates;
                        }
                        else
                        {
                            ex.PlateMath =
                                PlateMath.MetricPlatesNoSplit;
                        }
                    }
                }

                Database.Update(ex);
            }

            foreach (Routine r in Database.Table<Routine>())
            {
                if (r.Name.Contains("imperial"))
                {
                    r.Name = r.Name.Replace("imperial", "metric");

                    Database.Update(r);
                }
            }
        }

        public static Dictionary<int, T> ToMap<T>(this IEnumerable<T> objs) where T : IIdentifiable
        {
            Dictionary<int, T> map = new Dictionary<int, T>();

            foreach (T obj in objs)
            {
                map[obj.ID] = obj;
            }

            return map;
        }

        public static void SaveEdits(this IRoutineResult routine_result, Dictionary<int, float> WeightEdits, Dictionary<int, int> RepsEdits)
        {
            Dictionary<int, IExerciseResult> ex_result_map = routine_result.ExerciseResults.ToMap();
            HashSet<int> ex_result_ids_to_save = new HashSet<int>();

            foreach (KeyValuePair<int, float> er_id_to_weight in WeightEdits)
            {
                int er_id = er_id_to_weight.Key;
                float new_weight = er_id_to_weight.Value;
                ex_result_map[er_id].Weight = new_weight;
                ex_result_ids_to_save.Add(er_id);
            }

            foreach (KeyValuePair<int, int> er_id_to_reps in RepsEdits)
            {
                int er_id = er_id_to_reps.Key;
                int new_reps = er_id_to_reps.Value;
                ex_result_map[er_id].RepCount = new_reps;
                ex_result_ids_to_save.Add(er_id);
            }

            foreach (int id in ex_result_ids_to_save)
            {
                routine_result.Database.Update((ExerciseResult)ex_result_map[id]);
            }
        }

        public static IEnumerable<IExerciseSets> SplitLockedSets(this IEnumerable<IExercise> exercises,
            int exercises_locked, out int sets_locked)
        {
            List<IExerciseSets> exercise_sets = new List<IExerciseSets>();

            IExercise first = exercises.FirstOrDefault();
            if (first == null)
            {
                sets_locked = 0;

                return exercise_sets;
            }
            IPOLDatabase Database = first.Database;

            IEnumerable<IExerciseSets> locked_sets =
                    ExerciseSets.Group(exercises.Take(exercises_locked), Database);
            IEnumerable<IExerciseSets> unlocked_sets =
                ExerciseSets.Group(exercises.Skip(exercises_locked), Database);
            sets_locked = locked_sets.Count();
            exercise_sets.AddRange(locked_sets);
            exercise_sets.AddRange(unlocked_sets);

            return exercise_sets;
        }

        public static List<IExerciseSets> RemoveZeroSets(this IEnumerable<IExerciseSets> exercise_sets)
        {
            List<IExerciseSets> result = new List<IExerciseSets>();

            foreach (IExerciseSets es in exercise_sets)
            {
                if (es.SetCount != 0)
                {
                    result.Add(es);
                    break;
                }
            }

            return result;
        }

        public static List<IExerciseSets> NormalizeExerciseSets(this IEnumerable<IExerciseSets> exercise_sets, 
            IPOLDatabase database)
        {
            return ExerciseSets.Regroup(exercise_sets.RemoveZeroSets(), database);
        }

        public static List<IExerciseSets> SaveExerciseSets(this IEnumerable<IExerciseSets> exercise_sets,
            IPOLDatabase database)
        {
            List<IExerciseSets> normalized = exercise_sets.NormalizeExerciseSets(database);
            foreach (IExerciseSets ex_sets in normalized)
            {
                database.InsertOrUpdateNoID<ExerciseSets>((ExerciseSets)ex_sets);
            }

            return normalized;
        }
    }
}