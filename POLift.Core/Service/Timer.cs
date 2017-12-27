using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public abstract class Timer : IDisposable
    {
        public bool IsRunning { get; protected set; } = false;

        public abstract void Start(Action callback, int time_period_ms);

        public virtual void Cancel()
        {
            IsRunning = false;
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}

