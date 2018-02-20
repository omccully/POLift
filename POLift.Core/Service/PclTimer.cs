using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public class PclTimer : Timer
    {
        public override void Start(Action callback, int time_period_ms)
        {
            IsRunning = true;

            Task.Run(async () => TimerLoop(callback, time_period_ms));


            /*    async () =>
            {
                while (true)
                {
                    await Task.Delay(time_period_ms);
                    if (!IsRunning)
                        break;
                    Task.Run(() => callback());
                }
            }*/

        }

        protected virtual async Task TimerLoop(Action callback, int time_period_ms)
        {
            System.Diagnostics.Debug.WriteLine("TimerLoop");
            while (true)
            {
                await Task.Delay(time_period_ms);
                System.Diagnostics.Debug.WriteLine("TimerLoop 1");
                if (!IsRunning)
                    break;
                System.Diagnostics.Debug.WriteLine("TimerLoop 2");
                Task.Run(() => callback());
                System.Diagnostics.Debug.WriteLine("TimerLoop 3");
            }
        }
    }
}
