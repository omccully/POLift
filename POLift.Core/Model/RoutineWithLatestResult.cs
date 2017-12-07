using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    using Service;

    public class RoutineWithLatestResult : IRoutineWithLatestResult
    {
        public IRoutine Routine { get; set; }
        public IRoutineResult LatestResult { get; set; }

        public RoutineWithLatestResult(IRoutine Routine, IRoutineResult LatestResult)
        {
            this.Routine = Routine;
            this.LatestResult = LatestResult;
        }
    }
}
