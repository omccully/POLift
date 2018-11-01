using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    class TrialPeriodSourceOfflineFailover : ITrialPeriodSource
    {
        
        const string TimeOfFirstLaunchKey = "license_manager.first_launch_time";

        // assumptions made about trial period
        // this must be changed if trial period is changed server-side
        public const int TrialPeriodDays = 15;
        public const int TrialPeriodSeconds = TrialPeriodDays * 86400;

        ITrialPeriodSource Inner;
        KeyValueStorage KeyValueStorage;

        public TrialPeriodSourceOfflineFailover(ITrialPeriodSource inner, KeyValueStorage kvs)
        {
            this.Inner = inner;
            this.KeyValueStorage = kvs;

            if (kvs != null)
            {
                if (kvs.GetInteger(TimeOfFirstLaunchKey, 0) == 0)
                {
                    // first launch time was never set

                    kvs.SetValue(TimeOfFirstLaunchKey,
                        (int)Helpers.UnixTimeStamp());
                }
            }
        }

        public async Task<int> SecondsRemainingInTrial()
        {
            try
            {
                return await Inner.SecondsRemainingInTrial();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                if (KeyValueStorage != null)
                {
                    long first_launch = KeyValueStorage.GetInteger(TimeOfFirstLaunchKey, 0);
                    System.Diagnostics.Debug.WriteLine("first_launch = " + first_launch);

                    if (first_launch != 0)
                    {
                        long trial_end_time = first_launch + TrialPeriodSeconds;
                        int sec_left = (int)(trial_end_time - Core.Service.Helpers.UnixTimeStamp());
                        System.Diagnostics.Debug.WriteLine("trial_end_time = " + trial_end_time + ", sec_left = " + sec_left);
                        return sec_left;
                    }
                }

                throw e;
            }
        }

        public async Task<TimeSpan> TimeRemainingInTrial()
        {
            return TimeSpan.FromSeconds((await SecondsRemainingInTrial()));
        }
    }
}
