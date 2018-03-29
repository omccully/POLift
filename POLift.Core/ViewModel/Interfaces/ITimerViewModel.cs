using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using POLift.Core.Service;

namespace POLift.Core.ViewModel
{
    public interface ITimerViewModel
    {
        INotificationService TimerFinishedNotificationService { get; }

        bool TimerIsStartable { get; set; }

        void StartTimer(int seconds_left);

        void CancelTimer(string msg = "");
    }
}
