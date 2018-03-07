using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public interface IWarmupRoutine
    {
        List<IWarmupSet> WarmupSets { get; }

        int GetLastRestPeriod(IExercise ex);
    }
}
