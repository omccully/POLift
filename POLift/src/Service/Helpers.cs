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
    static class Helpers
    {
        public static void DisplayError(Activity activity, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(activity);
            dialog.SetMessage(message);
            dialog.SetNeutralButton("Ok", delegate { });
            dialog.Show();
        }
    }
}