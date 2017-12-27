using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    public interface ITimerViewModel
    {
        bool TimerIsStartable { get; set; }

        void StartTimer(int seconds_left);
    }
}
