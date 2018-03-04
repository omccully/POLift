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
    class AndroidVibrator : IVibrator
    {
        Vibrator vibrator;
        public AndroidVibrator(Context context)
        {
            vibrator = (Vibrator)
                context.GetSystemService(Context.VibratorService);
        }

        public void Vibrate()
        {
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);
            System.Threading.Thread.Sleep(300);
            vibrator.Vibrate(200);
        }
    }
}