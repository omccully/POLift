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
                CreateTableIfNotExists<Exercise>();
                CreateTableIfNotExists<Routine>();
                //CreateTableIfNotExists<ExerciseResult>();
                //CreateTableIfNotExists<RoutineResult>();

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
                int ID = Connection.Insert(obj);
                obj.ID = ID;
                return ID;
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