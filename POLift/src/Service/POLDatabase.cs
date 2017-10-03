using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using POLift.Model;

using SQLite;

namespace POLift.Service
{
    public delegate void DatabaseOperation(SQLiteConnection Connection);

    static class POLDatabase
    {
        static string DatabaseFileName = "database.db3";
        static string DatabaseDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        static string DatabasePath = Path.Combine(DatabaseDirectory, DatabaseFileName);

        static object Locker = new object();

        // use Invoke instead of using Connection publicly
        static SQLiteConnection Connection = new SQLiteConnection(DatabasePath);

        public static string Error = "";

        static void ClearDatabase()
        {
            Connection.DropTable<Exercise>();
            Connection.DropTable<Routine>();
            Connection.DropTable<ExerciseSets>();
            Connection.DropTable<ExerciseResult>(); 
            Connection.DropTable<RoutineResult>();
        }

        static void InitializeDatabase()
        {
            CreateTableIfNotExists<Exercise>();
            CreateTableIfNotExists<Routine>();
            CreateTableIfNotExists<ExerciseSets>();
            CreateTableIfNotExists<ExerciseResult>();
            CreateTableIfNotExists<RoutineResult>();
        }

        static POLDatabase()
        {
            // comment this out unless testing
            //ClearDatabase();

            InitializeDatabase();
        }

        public static void Update(IIdentifiable obj)
        {
            lock (Locker)
            {
                Connection.Update(obj);
            }
        }

        public static void Invoke(DatabaseOperation operation)
        {
            lock(Locker)
            {
                operation?.Invoke(Connection);
            }
        }

        public static void CreateTableIfNotExists<T>()
        {
            try
            {
                lock (Locker)
                {
                    Connection.CreateTable<T>();
                }
            }
            catch(SQLiteException)
            {

            }
        }

        public static int Insert(IIdentifiable obj)
        {
            lock(Locker)
            {
                return Connection.Insert(obj);
            }
        }

        /// <summary>
        /// This should not be used for tables that have constraints that may prevent insertion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if new row inserted, false is old row was updated</returns>
        public static bool InsertOrUpdate(IIdentifiable obj)
        {
            lock (Locker)
            {
                if(obj.ID == 0 || Connection.Update(obj) == 0)
                {
                    Connection.Insert(obj);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// For tables that have constraints
        /// </summary>
        /// <param name="obj">obj should implement Equals, and
        /// obj's table should have an index between
        /// all of the properties that make obj unique</param>
        /// <returns>True if new row was inserted, otherwise false</returns>
        public static bool InsertOrUndeleteAndUpdate<T>(T obj) where T : class, IDeletable, new()
        {
            lock (Locker)
            {
                try
                {
                    Connection.Insert(obj);
                    return true;
                }
                catch(SQLiteException e)
                {
                    if(e.Result == SQLite3.Result.Constraint)
                    {
                        // obj already exists based on constraint
                        // find obj's ID

                        var table = Connection.Table<T>();

                        T existing = null;
                        foreach(T ex in table)
                        {
                            if(ex.Equals(obj))
                            {
                                existing = ex;
                                break;
                            }
                        }

                        if(existing == null)
                        {
                            throw new InvalidOperationException("This object does not exist in the database table.");
                        }

                        obj.ID = existing.ID;

                        Connection.Update(obj);


                        return true;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static bool HideDeletable<T>(T obj) where T : IDeletable, new()
        {
            obj.Deleted = true;
            if(obj.ID == 0)
            {
                if (!FillIDValue<T>(obj)) return false;
            }

            // update Deleted=true where ID=id
            Connection.Update(obj);
            return true;
        }

        public static bool FillIDValue<T>(T obj) where T : IIdentifiable, new()
        {
            try
            {
                T found = Table<T>().First(o => o.Equals(obj));
                obj.ID = found.ID;
                return true;
            }
            catch(InvalidOperationException)
            {
                return false;
            }
        }

        public static TableQuery<T> Table<T>() where T : new()
        {
            lock(Locker)
            {
                return Connection.Table<T>();
            }
        }

        public static List<T> TableWhereUndeleted<T>() where T : IDeletable, new()
        {
            lock (Locker)
            {
                /*var tab = Connection.Table<T>();
                var were = tab.Where(e => !e.Deleted);
                return were.ToList();*/

                List<T> results = new List<T>();

                foreach(T item in Connection.Table<T>())
                {
                    if (!item.Deleted) results.Add(item);
                }

                return results;
            }
        }

        public static T ReadByID<T>(int ID) where T : new()
        {
            lock(Locker)
            {
                return Connection.Get<T>(ID);
            }
        }

        public static IEnumerable<T> ParseIDString<T>(string IDs) where T : class, new()
        {
            return IDs.Split(',').Select(id_str =>
            {
                try
                {
                    int id = Int32.Parse(id_str);
                    return POLDatabase.ReadByID<T>(id);
                }
                catch (FormatException)
                {
                    return null;
                }
            }).Where(e => e != null);
        }
    }
}