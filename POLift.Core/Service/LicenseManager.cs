using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace POLift.Core.Service
{
    public class LicenseManager : ILicenseManager
    {
        public string ProductID { get; set; } = "polift_license";

        public const int TrialPeriodDays = 15;
        public const int TrialPeriodSeconds = TrialPeriodDays * 86400;

        const string HasLicenseConfirmedKey = "license_manager.has_licence_confirmed";
        const string TimeOfFirstLaunchKey = "license_manager.first_launch_time";

        public readonly string DeviceID;

        public bool ShowAds { get; set; } = false;

        string LicenseLookupURL
        {
            get
            {
                string device_id_encoded = WebUtility.UrlEncode(DeviceID);
                string domain = "polift-app.com";
                string path = $"polift/check_license.php?device_id={device_id_encoded}";
                return $"http://{domain}/{path}";
            }
        }

        KeyValueStorage _KeyValueStorage;
        public KeyValueStorage KeyValueStorage
        {
            get
            {
                return _KeyValueStorage;
            }
            set
            {
                _KeyValueStorage = value;
                if (value != null)
                {
                    if(value.GetInteger(TimeOfFirstLaunchKey, 0) == 0)
                    {
                        // first launch time was never set

                        value.SetValue(TimeOfFirstLaunchKey,
                            (int)Helpers.UnixTimeStamp());
                    }
                }
            }
        }

        public LicenseManager(string device_id, KeyValueStorage kvs = null)
        {
            this.DeviceID = device_id;

            lazy_SecondsRemainingInTrial = new Lazy<Task<int>>(
                SecondsRemainingInTrialFromServer_NotCached);

            lazy_CheckLicense = new Lazy<Task<bool>>(
                CheckLicenseStrict_NotCached);

            KeyValueStorage = kvs;

#if DEBUG
            CrossInAppBilling.Current.InTestingMode = true;
#endif
        }

        public async Task<bool> IsInTrialPeriod()
        {
            return (await SecondsRemainingInTrial()) > 0;
        }

        Lazy<Task<int>> lazy_SecondsRemainingInTrial;
        private async Task<int> SecondsRemainingInTrialFromServer_NotCached()
        {
            WebRequest web_request = HttpWebRequest.Create(LicenseLookupURL);
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

        public async Task<int> SecondsRemainingInTrialInner()
        {
            try
            {
                return await lazy_SecondsRemainingInTrial.Value;
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

        public async Task<int> SecondsRemainingInTrial()
        {
            int result = await SecondsRemainingInTrialInner();
            if (result <= 0)
            {
                ShowAds = true;
            }
            return result;
        }

        Lazy<Task<bool>> lazy_CheckLicense;
        /// <summary>
        /// If fails, it gives the user the benefit of the doubt if default_result=true
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckLicense(bool default_result = true)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Checking license...");
                if (KeyValueStorage.GetBoolean(HasLicenseConfirmedKey, false))
                {
                    System.Diagnostics.Debug.WriteLine("Using license from preferences");
                    return true;
                }
                return await lazy_CheckLicense.Value;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine("Using license from preferences, defaulting " + default_result);
                return KeyValueStorage.GetBoolean(HasLicenseConfirmedKey, default_result);
            }
        }

        public bool CheckLicenseCached(bool default_result = false)
        {
            return KeyValueStorage.GetBoolean(HasLicenseConfirmedKey, default_result);
        }

        protected virtual async Task<bool> CheckLicenseStrict_NotCached()
        {
            try
            {
                var connected = await CrossInAppBilling.Current.ConnectAsync();
                if (!connected)
                {
                    System.Diagnostics.Debug.WriteLine("(c)FAILED TO CONNECT TO BILLING");
                    throw new FailedToConnectToBillingException();
                }
                System.Diagnostics.Debug.WriteLine("(C)CONNECTED");

                IEnumerable<InAppBillingPurchase> purchases =
                    await CrossInAppBilling.Current.GetPurchasesAsync(
                        ItemType.InAppPurchase);

                System.Diagnostics.Debug.WriteLine("purchase count = " + purchases.Count());

                foreach (InAppBillingPurchase purchase in purchases)
                {
                    System.Diagnostics.Debug.WriteLine($"{purchase.ProductId} purchase = " + purchase);
                    if (purchase.ProductId == ProductID)
                    {
                        if (KeyValueStorage != null)
                        {
                            KeyValueStorage.SetValue(HasLicenseConfirmedKey, true);
                        }

                        return true;
                    }
                }
                if (KeyValueStorage != null)
                {
                    KeyValueStorage.SetValue(HasLicenseConfirmedKey, false);
                }
                return false;
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if user bought the license</returns>
        public virtual async Task<bool> PromptToBuyLicense()
        {
            try
            {
                //CrossInAppBilling.Current.InTestingMode = true;
                var connected = await CrossInAppBilling.Current.ConnectAsync();
                if (!connected)
                {
                    System.Diagnostics.Debug.WriteLine("FAILED TO CONNECT TO BILLING");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine("CONNECTED");
                var purchase = await CrossInAppBilling.Current.PurchaseAsync(ProductID,
                    ItemType.InAppPurchase, "");
                
                if (purchase != null)
                {
                    if (KeyValueStorage != null)
                    {
                        KeyValueStorage.SetValue(HasLicenseConfirmedKey, true);
                    }
                    return true;
                }
            }
            /*catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }*/
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }

            return false;
        }
    }
}
