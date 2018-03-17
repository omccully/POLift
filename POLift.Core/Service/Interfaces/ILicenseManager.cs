using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public interface ILicenseManager
    {
        string ProductID { get; set; }

        bool ShowAds { get; set; }

        KeyValueStorage KeyValueStorage { get; set; }

        Task<bool> IsInTrialPeriod();

        Task<int> SecondsRemainingInTrial();

        Task<bool> CheckLicense(bool default_result = true);

        Task<bool> PromptToBuyLicense();
    }
}
