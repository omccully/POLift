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

namespace POLift.Service
{
    public class EventDrivenAdListener : AdListener
    {
        public event EventHandler<EventArgs> AdClosed;
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            AdClosed?.Invoke(this, new EventArgs());
        }

        public event EventHandler<AdFailedToLoadEventArgs> AdFailedToLoad;
        public override void OnAdFailedToLoad(int errorCode)
        {
            base.OnAdFailedToLoad(errorCode);
            AdFailedToLoad?.Invoke(this, new AdFailedToLoadEventArgs(errorCode));
        }

        public event EventHandler<EventArgs> AdLeftApplication;
        public override void OnAdLeftApplication()
        {
            base.OnAdLeftApplication();
            AdLeftApplication?.Invoke(this, new EventArgs());

        }

        public event EventHandler<EventArgs> AdLoaded;
        public override void OnAdLoaded()
        {
            base.OnAdLoaded();
            AdLoaded?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> AdOpened;
        public override void OnAdOpened()
        {
            base.OnAdOpened();
            AdOpened?.Invoke(this, new EventArgs());
        }
    }
}