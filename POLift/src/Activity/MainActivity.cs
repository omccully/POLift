using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Telephony;
using Android.Provider;

using Microsoft.Practices.Unity;

using Plugin.InAppBilling;

namespace POLift
{
    using Model;
    using Service;

    [Activity(Label = "POLift", MainLauncher = true /*, Icon ="@mipmap/polift"*/)]
    class MainActivity : ToolbarAndDrawerActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SupportActionBar.Title = "Progressive Overload Lifting";

            if (savedInstanceState == null)
            {
                SwitchToFragment(new MainFragment(), false);
            }
            else
            {
                RestoreLastFragment();
            }

            //TelephonyManager tm = (TelephonyManager)GetSystemService(Context.TelephonyService);
            //Helpers.DisplayError(this, tm.DeviceId);
            // 
            /*string id =
                Settings.Secure.GetString(
                    ApplicationContext.ContentResolver,
                    Settings.Secure.AndroidId);
            Helpers.DisplayError(this, id);*/


            


        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }
    }
}