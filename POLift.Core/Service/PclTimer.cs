using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    internal class PclTimer : Timer
    {
        public override void Start(Action callback, int time_period_ms)
        {
            IsRunning = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(time_period_ms);
                    if (!IsRunning)
                        break;
                    Task.Run(() => callback());
                }
            });
        }
    }
}
