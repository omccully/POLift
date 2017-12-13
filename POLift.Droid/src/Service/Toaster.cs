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

using POLift.Core.Service;

namespace POLift.Droid.Service
{
    public class Toaster : IToaster
    {
        Context context;

        public Toaster(Context context)
        {
            this.context = context;
        }

        public void DisplayMessage(string message)
        {
            Toast.MakeText(context, message, ToastLength.Long).Show();
        }

        public void DisplayError(string message)
        {
            DisplayMessage(message);
        }
    }
}