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
using Android.Gms.Ads;

namespace POLift
{
    using Service;

    [Activity(Label = "Advertisement")]
    public class FullScreenAdActivity : Activity
    {
        InterstitialAd mInterstitialAd;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.FullScreenAd);
            /*
            AdView ad_view = FindViewById<AdView>(Resource.Id.full_ad);
            var adRequest = new AdRequest.Builder().Build();
            ad_view.LoadAd(adRequest);*/

            EventDrivenAdListener ad_listener = new EventDrivenAdListener();
            ad_listener.AdLoaded += Ad_listener_AdLoaded;
            ad_listener.AdClosed += Ad_listener_AdClosed;

            mInterstitialAd = new InterstitialAd(this);
            mInterstitialAd.AdUnitId = "ca-app-pub-1015422455885077/5168885337";
            mInterstitialAd.LoadAd(new AdRequest.Builder().Build());
            //mInterstitialAd.Show();
            mInterstitialAd.AdListener = ad_listener;


        }

        private void Ad_listener_AdClosed(object sender, EventArgs e)
        {
            Finish();
        }

        private void Ad_listener_AdLoaded(object sender, EventArgs e)
        {
            mInterstitialAd.Show();
        }
    }
}