using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public interface IRoutineWithLatestResult
    {
        IRoutine Routine { get; set; }
        IRoutineResult LatestResult { get; set; }

    }
}
