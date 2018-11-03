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
        const string HasLicenseConfirmedKey = "license_manager.has_licence_confirmed";
        public string ProductID { get; set; } = "polift_license";

        public readonly string DeviceID;

        public bool ShowAds { get; set; } = false;

        ITrialPeriodSource TrialPeriodSource;
        public KeyValueStorage KeyValueStorage { get; set; }

        public LicenseManager(string device_id, KeyValueStorage kvs = null)
        {
            this.DeviceID = device_id;

            lazy_CheckLicense = new Lazy<Task<bool>>(
                CheckLicenseStrict_NotCached);

            KeyValueStorage = kvs;

            ITrialPeriodSource CachedTrialPeriodSource = new TrialPeriodSourceCacher(
                new CloudServiceTrialPeriodSource(device_id));
            TrialPeriodSource = 
                new TrialPeriodSourceOfflineFailover(CachedTrialPeriodSource, kvs);

#if DEBUG
            CrossInAppBilling.Current.InTestingMode = true;
#endif
        }

        public async Task<bool> IsInTrialPeriod()
        {
            return (await SecondsRemainingInTrial()) > 0;
        }

        public async Task<int> SecondsRemainingInTrial()
        {
            int result = await TrialPeriodSource.SecondsRemainingInTrial();
            System.Diagnostics.Debug.WriteLine("Final SecondsRemainingInTrial Result = " + result);
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
