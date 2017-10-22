using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Service
{
    using Model;

    public interface IPOLDatabase
    {
        void ClearDatabase();

        void InitializeDatabase();

        void ResetDatabase();

        void Update(IIdentifiable obj);

       // void Invoke(DatabaseOperation operation);

        void CreateTableIfNotExists<T>();

        int Insert(IIdentifiable obj);

        bool InsertOrUpdateByID(IIdentifiable obj);

        bool InsertOrUpdateNoID<T>(T obj) where T : class, IIdentifiable, new();

        bool InsertOrUndeleteAndUpdate<T>(T obj) where T : class, IDeletable, new();

        bool HideDeletable<T>(T obj) where T : IDatabaseObject, IDeletable, new();

        bool FillIDValue<T>(T obj) where T : IDatabaseObject, IIdentifiable, new();

        IEnumerable<T> Table<T>() where T : IDatabaseObject, new();

        IEnumerable<T> TableWhereUndeleted<T>() where T : IDatabaseObject, IDeletable, new();

        T ReadByID<T>(int ID) where T : IDatabaseObject, new();

        bool Delete<T>(int ID);

        IEnumerable<T> ParseIDs<T>(IEnumerable<int> IDs) where T : class, IDatabaseObject, new();

        IEnumerable<T> ParseIDString<T>(string IDs) where T : class, IDatabaseObject, new();

        void ImportDatabase(IPOLDatabase other_database);
    }
}