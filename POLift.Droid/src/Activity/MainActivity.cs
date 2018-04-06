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
using Android.Preferences;

using Microsoft.Practices.Unity;

using Plugin.InAppBilling;

namespace POLift.Droid
{
    using Core.Model;
    using Core.Service;
    using Droid.Service;

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
        }

        protected override void OnStart()
        {
            base.OnStart();

            AndroidHelpers.SetActivityDepth(this, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }
    }
}