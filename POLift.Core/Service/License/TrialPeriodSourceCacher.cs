using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    class TrialPeriodSourceCacher : ITrialPeriodSource
    {
        ITrialPeriodSource Inner;
        Lazy<Task<int>> lazy_InnerSecondsRemainingInTrial;

        public TrialPeriodSourceCacher(ITrialPeriodSource inner)
        {
            this.Inner = inner;
            lazy_InnerSecondsRemainingInTrial = 
                new Lazy<Task<int>>(inner.SecondsRemainingInTrial);
        }
        
        public async Task<int> SecondsRemainingInTrial()
        {
            return await lazy_InnerSecondsRemainingInTrial.Value;
        }

        public async Task<TimeSpan> TimeRemainingInTrial()
        {
            return TimeSpan.FromSeconds((await SecondsRemainingInTrial()));
        }
    }
}
