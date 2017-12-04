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

namespace POLift.Droid.Service
{
    public class AdFailedToLoadEventArgs
    {
        public readonly int ErrorCode;

        public AdFailedToLoadEventArgs(int error_code)
        {
            ErrorCode = error_code;
        }
    }
}