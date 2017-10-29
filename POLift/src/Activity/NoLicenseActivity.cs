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

namespace POLift
{
    [Activity(Label = "NoLicenseActivity")]
    public class NoLicenseActivity : Activity
    {
        TextView InfoTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.NoLicense);

            InfoTextView = FindViewById<TextView>(Resource.Id.InfoTextView);

            StringBuilder info = new StringBuilder();

            info.AppendLine("Dear user: ");
            info.AppendLine();
            info.Append("We hope that you have been finding this app helpful so far. ");
            info.Append("Unfortunately, to ");
            info.Append("To provide this ad-free app, unfortunately, we must charge ");
            info.Append("To provide this ad-free app, unfortunately, we must charge ");

            //InfoTextView.Text = 

        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();
        }
    }
}