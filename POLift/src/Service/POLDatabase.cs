using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using POLift.Model;

using SQLite;

namespace POLift.Service
{
    class POLDatabase : IPOLDatabase
    {
        string FilePath;

        object Locker = new object();

        SQLiteConnection Connection;

        public POLDatabase(string file_path)
        {
            this.FilePath = file_path;

            Connection = new SQLiteConnection(file_path);

            InitializeDatabase();
            //ResetDatabase();
            

        }

        public void ClearDatabase()
        {
            lock (Locker)
            {
                Connection.DropTable<Exercise>();
                Connection.DropTable<Routine>();
                Connection.DropTable<ExerciseSets>();
                Connection.DropTable<ExerciseResult>();
                Connection.DropTable<RoutineResult>();
            }
        }

        public void InitializeDatabase()
        {
            CreateTableIfNotExists<Exercise>();
            CreateTableIfNotExists<Routine>();
            CreateTableIfNotExists<ExerciseSets>();
            CreateTableIfNotExists<ExerciseResult>();
            CreateTableIfNotExists<RoutineResult>();
        }

        public void ApplyConstaints()
        {
            Connection.Execute("CREATE UNIQUE INDEX \"UniqueGroupExercise\" on \"Exercise\"(\"Name\", \"MaxRepCount\", \"WeightIncrement\", \"RestPeriodSeconds\")");
            Connection.Execute("CREATE UNIQUE INDEX \"UniqueGroupExerciseSets\" on \"ExerciseSets\"(\"SetCount\", \"ExerciseID\")");
            Connection.Execute("CREATE UNIQUE INDEX \"UniqueGroupRoutine\" on \"Routine\"(\"Name\", \"ExerciseSetIDs\")");
        }


        public void ResetDatabase()
        {
            ClearDatabase();
            InitializeDatabase();
        }


        public void Update(IIdentifiable obj)
        {
            lock (Locker)
            {
                Connection.Update(obj);
            }
        }

        public void CreateTableIfNotExists<T>()
        {
            try
            {
                lock (Locker)
                {
                    Connection.CreateTable<T>();
                }
            }
            catch (SQLiteException)
            {

            }
        }

        public int Insert(IIdentifiable obj)
        {
            lock (Locker)
            {
                return Connection.Insert(obj);
            }
        }

        /// <summary>
        /// May through SQLiteException if used on
        /// tables that have constraints that may prevent insertion
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if new row inserted, false is old row was updated</returns>
        public bool InsertOrUpdateByID(IIdentifiable obj)
        {
            lock (Locker)
            {
                if (obj.ID == 0 || Connection.Update(obj) == 0)
                {
                    Connection.Insert(obj);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// For tables that have constraints, and for objects that do not have a known ID
        /// </summary>
        /// <param name="obj">obj should implement Equals, and
        /// obj's table should have an index between
        /// all of the properties that make obj unique</param>
        /// <returns>True if new row was inserted, otherwise false</returns>
        public bool InsertOrUpdateNoID<T>(T obj) where T : class, IIdentifiable, new()
        {
            lock (Locker)
            {
                try
                {
                    Connection.Insert(obj);
                    return true;
                }
                catch (SQLiteException e)
                {
                    if (e.Result == SQLite3.Result.Constraint)
                    {
                        // obj already exists based on constraint
                        // find obj's ID

                        var table = Connection.Table<T>();

                        T existing = null;
                        foreach (T ex in table)
                        {
                            if (ex.Equals(obj))
                            {
                                existing = ex;
                                break;
                            }
                        }

                        if (existing == null)
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

        public bool InsertOrUndeleteAndUpdate<T>(T obj) where T : class, IDeletable, new()
        {
            obj.Deleted = false;
            return InsertOrUpdateNoID(obj);
        }


        public bool HideDeletable<T>(T obj) where T : IDatabaseObject, IDeletable, new()
        {
            obj.Deleted = true;
            if (obj.ID == 0)
            {
                if (!FillIDValue<T>(obj)) return false;
            }

            // update Deleted=true where ID=id
            Connection.Update(obj);
            return true;
        }

        public bool FillIDValue<T>(T obj) where T : IDatabaseObject, IIdentifiable, new()
        {
            try
            {
                T found = Table<T>().First(o => o.Equals(obj));
                obj.ID = found.ID;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public IEnumerable<T> Table<T>() where T : IDatabaseObject, new()
        {
            lock (Locker)
            {
                var tab = Connection.Table<T>();

                List<T> result = new List<T>();
                foreach(T item in tab)
                {
                    item.Database = this;
                    result.Add(item);
                }

                //var list = new List<T>(tab);

                //return list.Select(r => {
                //    r.Database = this;
                //    return r;
                //});

                return result;
            }
        }

        public IEnumerable<T> TableWhereUndeleted<T>() where T : IDatabaseObject, IDeletable, new()
        {
            lock (Locker)
            {
                /*var tab = Connection.Table<T>();
                var were = tab.Where(e => !e.Deleted);
                return were.ToList();*/

                /*List<T> results = new List<T>();

                foreach (T item in Connection.Table<T>())
                {
                    if (!item.Deleted) results.Add(item);
                }

                return results;*/

                return Table<T>().Where(r => !r.Deleted);
            }
        }

        public T ReadByID<T>(int ID) where T : IDatabaseObject, new()
        {
             
            lock (Locker)
            {
                T obj = Connection.Get<T>(ID);
                obj.Database = this;
                return obj;
            }
        }

        public bool Delete<T>(int ID)
        {
            lock (Locker)
            {
                return Connection.Delete<T>(ID) > 0;
            }
        }

        public IEnumerable<T> ParseIDs<T>(IEnumerable<int> IDs) where T : class, IDatabaseObject, new()
        {
            return IDs.Select(id => ReadByID<T>(id));
        }

        public IEnumerable<T> ParseIDString<T>(string IDs) where T : class, IDatabaseObject, new()
        {
            return ParseIDs<T>(IDs.Split(',').Select(id_str =>
            {
                try
                {
                    return Int32.Parse(id_str);
                }
                catch (FormatException)
                {
                    return 0;
                }
            }).Where(e => e != 0));
        }

        public void ImportDatabase(IPOLDatabase other_database)
        {
            // prune the original db first.

            // lookup for old to new IDs
            Dictionary<int, int> ExerciseLookup = 
                Exercise.Import(other_database.Table<Exercise>(), this);
            
            Dictionary<int, int> ExerciseSetsLookup = ExerciseSets.Import(
                other_database.Table<ExerciseSets>(), this, ExerciseLookup);
            
            Dictionary<int, int> RoutineLookup = Routine.Import(
                other_database.Table<Routine>(), this, ExerciseSetsLookup);

            Dictionary<int, int> ExerciseResultLookup = ExerciseResult.Import(
                other_database.Table<ExerciseResult>(), this, ExerciseLookup);

            RoutineResult.Import(other_database.Table<RoutineResult>(),
                this, RoutineLookup, ExerciseResultLookup);
        }

        public void ImportDatabaseFromFile(string file_path)
        {
            ImportDatabase(new POLDatabase(file_path));
        }

        public void PruneByConstaints()
        {
            Dictionary<int, int> exercise_lookup = 
                Exercise.PruneByConstaints(this);

            Dictionary<int, int> exercise_sets_lookup =
                 ExerciseSets.PruneByConstaints(this, exercise_lookup);

            Dictionary<int, int> routine_lookup =
                Routine.PruneByConstaints(this, exercise_sets_lookup);

            RoutineResult.TranslateRoutineIDs(this, routine_lookup);

            ExerciseResult.TranslateExerciseIDs(this, exercise_lookup);
        }
    }
}