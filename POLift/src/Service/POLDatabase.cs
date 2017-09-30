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

        static POLDatabase()
        {
            try
            {
                //Connection.DropTable<Exercise>();
                //Connection.DropTable<Routine>();

                CreateTableIfNotExists<Exercise>();
                CreateTableIfNotExists<Routine>();
                CreateTableIfNotExists<ExerciseSets>();
                CreateTableIfNotExists<ExerciseResult>();
                CreateTableIfNotExists<RoutineResult>();

                if (Table<Exercise>().Count() == 0)
                {
                    Insert(new Exercise("Flat barbell bench press", 6));
                }
            }
            catch(Exception e)
            {
                Error = e.ToString();
                string err = Error;
                int i = 5;
                i++;
            }
            
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
        /// 
        /// </summary>
        /// <param name="obj">obj should implement ==, and
        /// obj's table should have an index between
        /// all of the properties that make obj unique</param>
        /// <returns>True if new row was inserted, otherwise false</returns>
        public static bool InsertOrUndelete<T>(T obj) where T : IDeletable, new()
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

                        T existing = Connection.Table<T>().First(o => o.Equals(obj));

                        obj.ID = existing.ID;

                        if(existing.Deleted && !obj.Deleted)
                        {
                            existing.Deleted = false;
                            Connection.Update(existing);
                        }

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

        public static T ReadByID<T>(int ID) where T : new()
        {
            lock(Locker)
            {
                return Connection.Get<T>(ID);
            }
        }
    }
}