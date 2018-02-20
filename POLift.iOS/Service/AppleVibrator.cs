using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using AudioToolbox;


using POLift.Core.Service;

namespace POLift.iOS.Service
{
    class AppleVibrator : IVibrator
    {
        public void Vibrate()
        {
            SystemSound.Vibrate.PlaySystemSound();
        }
    }
}