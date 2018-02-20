using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;

namespace POLift.iOS.Service
{
    public class BackgroundTimer : PclTimer
    {
        public override void Start(Action callback, int time_period_ms)
        {
            IsRunning = true;

            System.Diagnostics.Debug.WriteLine("BackgroundTimer.Start");
            StartAsync(callback, time_period_ms);
        }

        async void StartAsync(Action callback, int time_period_ms)
        {
            System.Diagnostics.Debug.WriteLine("StartAsync");
            nint task_id = UIApplication.SharedApplication.BeginBackgroundTask(
                delegate { });
            System.Diagnostics.Debug.WriteLine("StartAsync1");
            await base.TimerLoop(callback, time_period_ms);
            System.Diagnostics.Debug.WriteLine("StartAsync2");
            UIApplication.SharedApplication.EndBackgroundTask(task_id);
            System.Diagnostics.Debug.WriteLine("StartAsync3");
        }
    }

}