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

namespace POLift.Service
{
    class POLDatabaseProvider : Android.Support.V4.Content.FileProvider
    {
        public Android.Net.Uri GetDatabaseURI(Context c)
        {
            //Java.IO.File export_file = new Java.IO.File(
            //    Android.OS.Environment.DataDirectory, Path.GetFileName(C.DatabasePath));
            Java.IO.File export_file = new Java.IO.File(C.DatabasePath);
            return GetUriForFile(c, "POLift.poliftdatabaseprovider", export_file);
        }



    }
}