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

using Microsoft.Practices.Unity;

namespace POLift
{
    using Service;

    static class C
    {
        public static UnityContainer ontainer { get; private set; }

        static string DatabaseFileName = "database.db3";
        static string DatabaseDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        public static string DatabasePath = Path.Combine(DatabaseDirectory, DatabaseFileName);

        static C()
        {
            ontainer = new UnityContainer();

           

            ontainer.RegisterInstance<IPOLDatabase>(new POLDatabase(DatabasePath));
        }
    }
}