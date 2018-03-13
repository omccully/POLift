using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;

namespace POLift.Core.ViewModel
{
    using Model;
    using Service;

    public class SideMenuViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase database;

        public KeyValueStorage KeyValueStorage;
        public ILicenseManager LicenseManager;
        public DialogService DialogService;
        public IToaster Toaster;
        public IMainThreadInvoker MainThreadInvoker;

        public SideMenuViewModel(INavigationService navigationService, IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.database = database;
        }

        public const string HasRatedAppStorageKey = "has_rated_app";
        public bool HasRatedApp
        {
            get
            {
                return KeyValueStorage.GetBoolean(
                    HasRatedAppStorageKey, false);
            }
        }

        public bool ShowRateApp
        {
            get
            {
                return !HasRatedApp;
            }
        }

        const string FirstLaunchForExternalProgramsStorageKey =
            "first_launch_for_external_programs";
        public void PromptUserForExternalProgramsIfFirstLaunch(Action navigate_action = null)
        {
            bool first_launch =
                KeyValueStorage.GetBoolean(FirstLaunchForExternalProgramsStorageKey, true);
            System.Diagnostics.Debug.WriteLine($"first_launch_for_external_programs = {first_launch}");

            if (first_launch)
            {
                DialogService.DisplayConfirmation("You can get started with the app right away " +
                    "by using one of the built-in weightlifting programs. " +
                    "They focus on compound lifts that have withstood " +
                    "the test of time. Would you like to select a " +
                    "built-in program now?",
                    delegate
                    {
                        FlagExternalProgramsResponse();
                        GetFreeWeightliftingPrograms(navigate_action);
                    },
                    delegate
                    {
                        FlagExternalProgramsResponse();
                        Toaster.DisplayMessage("If you change your mind, you " +
                            "can get one of the built-in working programs " +
                            "at any time from the navigation drawer by swiping " +
                            "from the left.");
                    });
            }
        }

        //Lazy<Task<bool>> lazy_LicenseNotPurchased;

        Navigation purchase_navigation = null;
        public async Task<Navigation> GetPurchaseLicenseNavigationLink()
        {
            if(await LicenseManager.CheckLicense(false))
            {
                // user already bought license
                return null;
            }

            string days_left_text = await DaysLeftText();
            // lifetime license

            purchase_navigation = new Navigation("Purchase" + days_left_text,
                PromptToBuyLicense);

            return purchase_navigation;
        }

        public void PromptToBuyLicense()
        {
            PromptToBuyLicenseAsync();
        }

        public async void PromptToBuyLicenseAsync()
        {
            try
            {
                if(await LicenseManager.PromptToBuyLicense())
                {
                    DisplayMessage("Purchase successful");

                    if(purchase_navigation != null)
                    {
                        purchase_navigation.Text = "License purchased";
                    }
                }
                else
                {
                    DisplayMessage("Purchase unsuccessful");
                }
            }
            catch (Exception e)
            {
                DisplayMessage(e.Message);
            }
        }

        void DisplayMessage(string msg )
        {
            System.Diagnostics.Debug.WriteLine("ERROR : " + msg);

            MainThreadInvoker.Invoke(delegate
            {
                DialogService.DisplayAcknowledgement(msg);
            });
        }

        async Task<string> DaysLeftText()
        {
            try
            {
                int sec_left = await LicenseManager.SecondsRemainingInTrial();
                int days_left = Math.Abs(sec_left / 86400);

                string plur = days_left == 1 ? "" : "s";

                if (sec_left > 0)
                {
                    return $" ({days_left} day{plur} left)";
                }
                else
                {
                    return $" (expired {days_left} day{plur} ago)";
                }
            }
            catch
            {
                return "";
            }
        }
    

        void FlagExternalProgramsResponse()
        {
            KeyValueStorage.SetValue(FirstLaunchForExternalProgramsStorageKey, false);
        }

        public const string HelpUrl = "https://reddit.com/r/POLift";

        public void SelectRoutineNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.MainPageKey);
        }

        public void ViewRecentSessionsNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.ViewRoutineResultsPageKey);
        }

        public void ViewOrmGraphsNavigate()
        {
            navigationService.NavigateTo(
                ViewModelLocator.OrmGraphPageKey);
        }

        public void GetFreeWeightliftingPrograms(Action navigate_action = null)
        {
            if(navigate_action == null)
            {
                navigationService.NavigateTo(
                    ViewModelLocator.SelectProgramToDownloadPageKey);
            }
            else
            {
                navigate_action();
            }
        }

        public void RecheckLicense()
        {
            RecheckLicenseAsync();
        }

        async void RecheckLicenseAsync()
        {
            bool has_license_confirmed = await LicenseManager.CheckLicense(false);

            string message = "";
            if(has_license_confirmed)
            {
                message = "Your license has been verified.";
            }
            else
            {
                message = "Your license failed to verify.";
            }

            if(MainThreadInvoker == null)
            {
                DialogService.DisplayAcknowledgement(message);
            }
            else
            {
                MainThreadInvoker.Invoke(delegate
                {
                    DialogService.DisplayAcknowledgement(message);
                });
            }
        }
    }
}
