using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Service
{
    public interface ILicenseManager
    {
        bool ShowAds { get; set; }

        ISharedPreferences BackupPreferences { get; set; }

        Task<bool> IsInTrialPeriod();

        Task<int> SecondsRemainingInTrial();

        Task<bool> CheckLicense(bool default_result = true);

        Task<bool> PromptToBuyLicense();

    }
}