using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using Plugin.CurrentActivity;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace POLift.Service
{
    public static class LicenseManager
    {
        public static readonly string ProductID = "polift_license";

        public static async Task<bool> IsInTrialPeriod(string device_id)
        {
            return (await SecondsRemainingInTrial(device_id)) > 0;
        }

        public static async Task<int> SecondsRemainingInTrial(string device_id)
        {
            
            string device_id_encoded = WebUtility.UrlEncode(device_id);
            string domain = "crystalmathlabs.com";
            string path = $"polift/check_license.php?device_id={device_id_encoded}";
            string url = $"http://{domain}/{path}";
            WebRequest web_request = HttpWebRequest.Create(url);

            WebResponse web_response = await web_request.GetResponseAsync();

            string str_response;

            using (Stream response_stream = web_response.GetResponseStream())
            {
                using (StreamReader reponse_reader = new StreamReader(response_stream))
                {
                    str_response = reponse_reader.ReadToEnd();
                }
            }

            return Int32.Parse(str_response);
            /*new Handler(this.MainLooper).Post(delegate
            {
                Helpers.DisplayError(CrossCurrentActivity.Current.Activity, str_response);
            });*/
        }

        /// <summary>
        /// If fails, it gives the user the benefit of the doubt (returns true)
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> CheckLicense()
        {
            // TODO: can't JUST give the user the benefit of the doubt because 
            // of connection failures. users can just turn off their wifi/shit 
            // to use the app. but maybe, if users do that, they're probably
            // kind of users who wouldn't want to buy the app anyway.

            try
            {
                
                CrossInAppBilling.Current.InTestingMode = true;
                var connected = await CrossInAppBilling.Current.ConnectAsync();
                if (!connected)
                {
                    System.Diagnostics.Debug.WriteLine("(c)FAILED TO CONNECT TO BILLING");
                    return true;
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
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                // assume true for user's convenience
                return true;
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
        public static async Task<bool> PromptToBuyLicense()
        {
            try
            {
                CrossInAppBilling.Current.InTestingMode = true;
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
                    return true;
                }
            }
            catch(Exception e)
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