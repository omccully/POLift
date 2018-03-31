using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Model;
using POLift.Core.Service;
using POLift.Core.ViewModel;
using Google.MobileAds;

namespace POLift.iOS.Controllers
{
    public abstract class PerformRoutineBaseController : UIViewController
    {
        Interstitial interstitial;

        protected abstract PerformBaseViewModel BaseVm { get; }

        protected TimerViewModel TimerVm
        {
            get
            {
                return ViewModelLocator.Default.Timer;
            }
        }

        public PerformRoutineBaseController(IntPtr handle) : base (handle)
        {
        }

        protected bool ShowAds => ViewModelLocator.Default.LicenseManager.ShowAds;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if(ShowAds)
            {
                CreateAndLoadInterstitial();
            }

            BaseVm.ResultSubmittedWithoutCompleting += BaseVm_ResultSubmittedWithoutCompleting;
        }

        public override void ViewDidUnload()
        {
            BaseVm.ResultSubmittedWithoutCompleting -= BaseVm_ResultSubmittedWithoutCompleting;

            base.ViewDidUnload();
        }

        private void CreateAndLoadInterstitial()
        {
            interstitial = new Interstitial("ca-app-pub-1015422455885077/8711276229");

            Request req = Request.GetDefaultRequest();
#if DEBUG
            req.TestDevices = new string[]
            {
                "5763FA36-B1DC-4B5A-8B3F-AD07DD5F988A"
            };
#endif

            interstitial.LoadRequest(req);

            interstitial.ScreenDismissed += Interstitial_ScreenDismissed;
        }

        private void Interstitial_ScreenDismissed(object sender, EventArgs e)
        {
            if (ShowAds)
            {
                CreateAndLoadInterstitial();
            }
        }

        Random randy = new Random();
        private void BaseVm_ResultSubmittedWithoutCompleting(object sender, EventArgs e)
        {
            TryShowFullScreenAd();
        }

        void TryShowFullScreenAd()
        {
            if (!ShowAds)
            {
                Console.WriteLine("ShowAds = false");
            }
            else if (interstitial != null && interstitial.IsReady)
            {
                int ran = randy.Next(4);

                if (ran == 0)
                {
                    Console.WriteLine("showing interstitial...");
                    interstitial.PresentFromRootViewController(this);
                }
                else
                {
                    Console.WriteLine("ran = " + ran);
                }
            }
            else
            {
                Console.WriteLine("ad wasn't ready");
            }
        }
    }
}