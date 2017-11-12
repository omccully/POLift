using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using POLift.Model;

using SQLite;

namespace POLift.Service
{
    public class POLDatabase : IPOLDatabase
    {
        string FilePath;

        object Locker = new object();

        SQLiteConnection Connection;

        const int CodeVersion = 1;
        const string DatabaseVersionLookupKey = "DATABASE_VERSION";
        public int DatabaseVersion
        {
            get
            {
                
                ValueLookup value_lookup = Get<ValueLookup>(DatabaseVersionLookupKey);
                return value_lookup.ValueInt;
            }
            set
            {
                ValueLookup value_lookup = new ValueLookup();
                value_lookup.LookupKey = DatabaseVersionLookupKey;
                value_lookup.ValueInt = value;
                lock (Locker)
                {
                    Connection.InsertOrReplace(value_lookup);
                }
            }
        }

        public POLDatabase(string file_path)
        {
            this.FilePath = file_path;

            Connection = new SQLiteConnection(file_path);
            
            InitializeDatabase();
           // System.Diagnostics.Debug.WriteLine("UPDATE Exercise SET ConsecutiveSetsForWeightIncrease=1 WHERE ConsecutiveSetsForWeightIncrease=0");
            //Connection.Execute("UPDATE Exercise SET ConsecutiveSetsForWeightIncrease=1 WHERE ConsecutiveSetsForWeightIncrease=0");
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
                Connection.DropTable<ValueLookup>();
                Connection.DropTable<ExerciseDifficulty>();
            }
        }

        public void InitializeDatabase()
        {
            CreateTableIfNotExists<Exercise>();
            CreateTableIfNotExists<Routine>();
            CreateTableIfNotExists<ExerciseSets>();
            CreateTableIfNotExists<ExerciseResult>();
            CreateTableIfNotExists<RoutineResult>();
            CreateTableIfNotExists<ExerciseDifficulty>();

            int dab_version;
            try
            {
                dab_version = DatabaseVersion;
                System.Diagnostics.Debug.WriteLine("dab version " + dab_version);
            }
            catch(SQLiteException)
            {
                CreateTableIfNotExists<ValueLookup>();
                System.Diagnostics.Debug.WriteLine("failed dab version");
                //Connection.CreateTable<ValueLookup>();
                dab_version = (DatabaseVersion = 0);
                System.Diagnostics.Debug.WriteLine("failed dab version2");
            }

            if(dab_version == 0)
            {
                // first migration, for ConsecutiveSetsForWeightIncrease
                lock (Locker)
                {
                    // SQLite may have added this column for us automatically
                    TryExecute("ALTER TABLE Exercise ADD ConsecutiveSetsForWeightIncrease INT NOT NULL");
                    Connection.Execute("DROP INDEX \"UniqueGroupExercise\"");
                    Connection.Execute("CREATE UNIQUE INDEX \"UniqueGroupExercise\" on \"Exercise\"(\"Name\", \"MaxRepCount\", \"WeightIncrement\", \"RestPeriodSeconds\", \"ConsecutiveSetsForWeightIncrease\")");
                    Connection.Execute("UPDATE Exercise SET ConsecutiveSetsForWeightIncrease=1");
                }
                DatabaseVersion = 1; // CodeVersion;
                System.Diagnostics.Debug.WriteLine("DatabaseVersion = " + CodeVersion);
            }
        }

        public void ApplyConstraints()
        {
            throw new NotImplementedException();
            TryExecute("CREATE UNIQUE INDEX \"UniqueGroupExercise\" on \"Exercise\"(\"Name\", \"MaxRepCount\", \"WeightIncrement\", \"RestPeriodSeconds\")");
            TryExecute("CREATE UNIQUE INDEX \"UniqueGroupExerciseSets\" on \"ExerciseSets\"(\"SetCount\", \"ExerciseID\")");
            TryExecute("CREATE UNIQUE INDEX \"UniqueGroupRoutine\" on \"Routine\"(\"Name\", \"ExerciseSetIDs\")");
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

        void TryExecute(string s)
        {
            try
            {
                lock (Locker)
                {
                    Connection.Execute(s);
                }
            }
            catch(Exception e)
            {
            }
        }

        public List<T> Query<T>(string query, params object[] args) where T : new()
        {
            lock(Locker)
            {
                return Connection.Query<T>(query, args);
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
            catch (SQLiteException e)
            {
                System.Diagnostics.Debug.WriteLine(typeof(T) + " " + e.ToString());
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

        public T Get<T>(object objp) where T : new()
        {
            lock (Locker)
            {
                T obj = Connection.Get<T>(objp);
                return obj;
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

        public void ImportRoutinesAndExercises(IPOLDatabase other_database)
        {
            // lookup for old to new IDs
            Dictionary<int, int> ExerciseLookup =
                Exercise.Import(other_database.Table<Exercise>(), this);

            Dictionary<int, int> ExerciseSetsLookup = ExerciseSets.Import(
                other_database.Table<ExerciseSets>(), this, ExerciseLookup);

            Dictionary<int, int> RoutineLookup = Routine.Import(
                other_database.Table<Routine>().Where(r => !r.Deleted), 
                this, ExerciseSetsLookup);
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
            throw new NotSupportedException("not tested");

            Dictionary<int, int> exercise_lookup = 
                Exercise.PruneByConstaints(this);
            System.Diagnostics.Debug.WriteLine(exercise_lookup.Count());

            Dictionary<int, int> exercise_sets_lookup =
                 ExerciseSets.PruneByConstaints(this, exercise_lookup);
            System.Diagnostics.Debug.WriteLine(exercise_sets_lookup.Count());

            Dictionary<int, int> routine_lookup =
                Routine.PruneByConstaints(this, exercise_sets_lookup);
            System.Diagnostics.Debug.WriteLine(routine_lookup.Count());

            RoutineResult.TranslateRoutineIDs(this, routine_lookup);

            ExerciseResult.TranslateExerciseIDs(this, exercise_lookup);
            System.Diagnostics.Debug.WriteLine("apply:");
            ApplyConstraints();
        }

        public void LowerCaseAllExercisesAndRoutines()
        {
            foreach(Exercise ex in this.Table<Exercise>())
            {
                ex.Name = ex.Name.ToLower();

                this.Update(ex);
            }

            foreach (Routine r in this.Table<Routine>())
            {
                r.Name = r.Name.ToLower();

                this.Update(r);
            }
        }
    }
}