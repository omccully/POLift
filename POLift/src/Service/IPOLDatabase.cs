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
    interface IPOLDatabase
    {
        void ClearDatabase();

        void InitializeDatabase();

        void ResetDatabase();

        void Update(IIdentifiable obj);

        void Invoke(DatabaseOperation operation);

        void CreateTableIfNotExists<T>();

        int Insert(IIdentifiable obj);

        bool InsertOrUpdateByID(IIdentifiable obj);

        bool InsertOrUpdateNoID<T>(T obj) where T : class, IIdentifiable, new();

        bool InsertOrUndeleteAndUpdate<T>(T obj) where T : class, IDeletable, new();

        bool HideDeletable<T>(T obj) where T : IDeletable, new();

        bool FillIDValue<T>(T obj) where T : IIdentifiable, new();

        TableQuery<T> Table<T>() where T : new();

        List<T> TableWhereUndeleted<T>() where T : IDeletable, new();

        T ReadByID<T>(int ID) where T : new();

        IEnumerable<T> ParseIDs<T>(IEnumerable<int> IDs) where T : class, new();

        IEnumerable<T> ParseIDString<T>(string IDs) where T : class, new();

        void ImportDatabase(IPOLDatabase other_database);
    }
}