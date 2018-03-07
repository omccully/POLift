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
            const int On = 200;
            const int Off = 300;
            for (int i = 0; i < 2; i++)
            {
                vibrator.Vibrate(On);
                System.Threading.Thread.Sleep(Off);
            }

            vibrator.Vibrate(On);
        }
    }
}