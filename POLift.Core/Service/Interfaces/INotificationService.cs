using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public interface INotificationService
    {
        void Notify();

        void Cancel();
    }
}
