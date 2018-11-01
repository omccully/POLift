using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    class CloudServiceTrialPeriodSource : ITrialPeriodSource
    {
        string DeviceId;
        TimeSpan Timeout = TimeSpan.FromSeconds(15);

        public CloudServiceTrialPeriodSource(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        public async Task<int> SecondsRemainingInTrial()
        {
            return (int)(await TimeRemainingInTrial()).TotalSeconds;
        }

        public async Task<TimeSpan> TimeRemainingInTrial()
        {
            SemaphoreSlim signal = new SemaphoreSlim(0, 1);

            POLiftCloudService.ServiceClient serviceClient = 
                new POLiftCloudService.ServiceClient();
            TimeSpan result;

            serviceClient.TimeLeftInTrialCompleted += (o, e) => 
            {
                if (e.Error != null) throw e.Error;
                result = e.Result;
                signal.Release();
            };
      
            serviceClient.TimeLeftInTrialAsync(DeviceId);

            bool success = await signal.WaitAsync(Timeout);

            if(!success)
            {
                throw new Exception("Cloud service request timed out after " +
                    Timeout.TotalSeconds + " seconds");
            }

            return result;
        }
    }
}
