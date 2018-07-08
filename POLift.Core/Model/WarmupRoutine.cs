using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class WarmupRoutine : IWarmupRoutine
    {
        public List<IWarmupSet> WarmupSets { get; private set; }

        int PercentLastRestPeriod;

        public WarmupRoutine(IEnumerable<IWarmupSet> warmup_sets, int percent_last_rest_period)
        {
            this.WarmupSets = new List<IWarmupSet>(warmup_sets);
            this.PercentLastRestPeriod = percent_last_rest_period;
        }

        public int GetLastRestPeriod(IExercise ex)
        {
            const int MaxRestPeriod = 150;
            return Math.Min(MaxRestPeriod,
                (ex.RestPeriodSeconds * PercentLastRestPeriod) / 100);
        }

        public static IWarmupRoutine Default
        {
            get
            {
                var sets = new IWarmupSet[]
                {
                    new WarmupSet(8, 50, 50),
                    new WarmupSet(8, 50, 50),
                    new WarmupSet(4, 70, 50),
                    new WarmupSet(1, 90, 50)
                };

                return new WarmupRoutine(sets, 65);
            }
        }
    }
}
