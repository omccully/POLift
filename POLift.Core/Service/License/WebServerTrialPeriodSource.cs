using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    class WebServerTrialPeriodSource : ITrialPeriodSource
    {
        string DeviceId;

        public WebServerTrialPeriodSource(string deviceId)
        {
            this.DeviceId = deviceId;
        }

        string TrialLookupUrl
        {
            get
            {
                string device_id_encoded = WebUtility.UrlEncode(DeviceId);
                string domain = "polift-app.com";
                string path = $"polift/check_license.php?device_id={device_id_encoded}";
                return $"http://{domain}/{path}";
            }
        }

        public async Task<int> SecondsRemainingInTrial()
        {
            WebRequest web_request = HttpWebRequest.Create(TrialLookupUrl);
            //web_request.Timeout = 3000;
            web_request.Proxy = null;

            System.Diagnostics.Debug.WriteLine("Querying license server");
            Task<WebResponse> response_task = web_request.GetResponseAsync();

            // force a 4000 ms timeout because something weird was happening
            if (await Task.WhenAny(response_task, Task.Delay(4000)) != response_task)
            {
                // timeout
                System.Diagnostics.Debug.WriteLine("await timeout");
                throw new TimeoutException();
            }
            System.Diagnostics.Debug.WriteLine("Received response from license server");

            WebResponse web_response = await response_task;

            using (Stream response_stream = web_response.GetResponseStream())
            {
                using (StreamReader reponse_reader = new StreamReader(response_stream))
                {
                    int secs = Int32.Parse(reponse_reader.ReadToEnd());
                    System.Diagnostics.Debug.WriteLine("seconds remaining in trial from server = " + secs);
                    return secs;
                }
            }
        }

        public async Task<TimeSpan> TimeRemainingInTrial()
        {
            return TimeSpan.FromSeconds((await SecondsRemainingInTrial()));
        }
    }
}
