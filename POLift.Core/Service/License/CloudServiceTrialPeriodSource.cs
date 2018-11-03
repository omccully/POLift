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
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(12);

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
            serviceClient.Endpoint.Binding.SendTimeout = Timeout;

            TimeSpan result;
            Exception exception = null;

            serviceClient.TimeLeftInTrialCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    exception = e.Error;
                }
                else
                {
                    result = e.Result;
                }

                signal.Release();

                serviceClient.CloseAsync();
            };

            serviceClient.TimeLeftInTrialAsync(DeviceId);

            bool success = await signal.WaitAsync(Timeout + TimeSpan.FromSeconds(10));

            if (!success || exception != null)
            {
                throw new Exception("Cloud service request timed out after " +
                   Timeout.TotalSeconds + " seconds");
            }

            return result;
        }
    }
}
