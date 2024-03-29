﻿using System;
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
using Android.Provider;



namespace POLift.Droid
{
    using Core.Service;
    using Service;

    static class C
    {
        static string DatabaseFileName = "database.db3";
        static string DatabaseDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        public static string DatabasePath = Path.Combine(DatabaseDirectory, DatabaseFileName);
        public static IPOLDatabase Database;

        static C()
        {
            try
            {
                  Database = new POLDatabase(
                        new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroidN(),
                        C.DatabasePath);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                throw e;
            }
        }
    }
}