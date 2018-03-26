using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace POLift.Core.Service
{
    public class PclTimer : Timer
    {
        public override void Start(Action callback, int time_period_ms)
        {
            IsRunning = true;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Task.Run(async () => TimerLoop(callback, time_period_ms));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        }

        protected virtual async Task TimerLoop(Action callback, int time_period_ms)
        {
            //System.Diagnostics.Debug.WriteLine("TimerLoop");
            while (true)
            {
                await Task.Delay(time_period_ms);
                System.Diagnostics.Debug.WriteLine("Timer ticked");
                if (!IsRunning)
                    break;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => callback());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }
}
