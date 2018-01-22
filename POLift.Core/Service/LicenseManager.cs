﻿using System;
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
        public static readonly string ProductID = "polift_license";
        public const int TrialPeriodSeconds = 30 * 86400;

        const string HasLicenseConfirmedKey = "license_manager.has_licence_confirmed";
        const string TimeOfFirstLaunchKey = "license_manager.first_launch_time";

        public readonly string DeviceID;

        public bool ShowAds { get; set; } = false;

        string LicenseLookupURL
        {
            get
            {
                string device_id_encoded = WebUtility.UrlEncode(DeviceID);
                string domain = "crystalmathlabs.com";
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
                    return Int32.Parse(reponse_reader.ReadToEnd());
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
                if (KeyValueStorage != null)
                {
                    long first_launch = KeyValueStorage.GetInteger(TimeOfFirstLaunchKey, 0);

                    if (first_launch != 0)
                    {
                        long trial_end_time = first_launch + TrialPeriodSeconds;
                        int sec_left = (int)(trial_end_time - Core.Service.Helpers.UnixTimeStamp());

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
        /// If fails, it gives the user the benefit of the doubt (returns true)
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckLicense(bool default_result = true)
        {
            try
            {
                return await lazy_CheckLicense.Value;
            }
            catch
            {
                return KeyValueStorage.GetBoolean(HasLicenseConfirmedKey, default_result);
            }
        }

        async Task<bool> CheckLicenseStrict_NotCached()
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
        public async Task<bool> PromptToBuyLicense()
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            finally
            {
                await CrossInAppBilling.Current.DisconnectAsync();
            }

            return false;
        }
    }
}