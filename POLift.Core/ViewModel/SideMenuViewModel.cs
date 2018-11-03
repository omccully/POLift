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
    using System.Collections.ObjectModel;

    public class SideMenuViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase database;

        public KeyValueStorage KeyValueStorage;
        public ILicenseManager LicenseManager;
        public DialogService DialogService;
        public IToaster Toaster;
        public IMainThreadInvoker MainThreadInvoker;

        public event EventHandler ShouldReloadMenu;

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
                        /*Toaster.DisplayMessage("If you change your mind, you " +
                            "can get one of the built-in working programs " +
                            "at any time from the navigation drawer by swiping " +
                            "from the left.");*/

                        InfoUserToLiftOnTheFly();
                    });
            }
            else
            {
                InfoUserToLiftOnTheFly();
            }
        }

        const string TutorialOnTheFlyKey = "tutorial_on_the_fly";
        public void InfoUserToLiftOnTheFly()
        {
            DialogService.DisplayAcknowledgementOnce("It is recommended that you create your routine before you " +
                "go to the gym. However, if you are at the gym now and want to get started right away, you can " +
                "create your routine \"on the fly.\"", TutorialOnTheFlyKey);
        }

        Navigation purchase_navigation = null;
        public async Task<Navigation> GetPurchaseLicenseNavigationLink()
        {
            if (LicenseManager.CheckLicenseCached(false))
            {
                // await LicenseManager.CheckLicense(false)
                // user already bought license
                return new Navigation("License purchased", delegate () { });
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
                if (await LicenseManager.PromptToBuyLicense())
                {
                    DisplayMessage("Purchase successful");

                    if (purchase_navigation != null)
                    {
                        purchase_navigation.Text = "License purchased";
                        purchase_navigation.Click = null;
                        ShouldReloadMenu?.Invoke(this, new EventArgs());
                    }
                }
                else
                {
                    DisplayMessage("Purchase unsuccessful");
                }
            }
            catch (Exception e)
            {
                DisplayMessage("Purchase error: " + e.Message);
            }
        }

        void DisplayMessage(string msg)
        {
            System.Diagnostics.Debug.WriteLine("SideMenuViewModel.ERROR : " + msg);

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

                bool is_in_trial = sec_left > 0;

                System.Diagnostics.Debug.WriteLine
                        ($"is_in_trial = {is_in_trial}, {sec_left} seconds left");


                int days_left = Math.Abs(sec_left / 86400);

                string plur = days_left == 1 ? "" : "s";

                if (is_in_trial)
                {
                    return $" ({days_left} day{plur} left)";
                }
                else
                {
                    return $" (expired {days_left} day{plur} ago)";
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Error determining trial period: " +
                    e.ToString() + "\n --- end---");
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

        public void PerformRoutineOnTheFlyNavigate()
        {
            // navigationService.NavigateTo(ViewModelLocator.PerformRoutinePageKey);
            ViewModelLocator.Default.PerformRoutine.PerformRoutineOnTheFly();
        }

        public void GetFreeWeightliftingProgramsWithNavigationService()
        {
            navigationService.NavigateTo(
                    ViewModelLocator.SelectProgramToDownloadPageKey);
        }

        public TimeSpan TimeSinceLastBackup
        {
            get
            {
                int last_backup_timestamp = KeyValueStorage.GetInteger(LastBackupTimeKey);
                if (last_backup_timestamp == 0) return TimeSpan.MaxValue;
                DateTime last_backup_time = Helpers.UnixTimeToDateTime(last_backup_timestamp);
                return DateTime.UtcNow - last_backup_time;
            }
        }

        public string BackupNavigationButtonText
        {
            get
            {
                TimeSpan time_since_backup = TimeSinceLastBackup;
                string text_since_backup = " (never done)";
                if (time_since_backup != TimeSpan.MaxValue)
                {
                    if (time_since_backup.TotalDays < 1)
                    {
                        text_since_backup = " (last done today)";
                    }
                    else if (time_since_backup.TotalDays < 2)
                    {
                        text_since_backup = " (last done yesterday)";
                    }
                    else
                    {
                        int days_rounded = (int)Math.Round(time_since_backup.TotalDays);
                        text_since_backup = $" (last done {days_rounded} days ago)";
                    }
                }
                return "Backup" + text_since_backup;
            }
        }

        const string LastBackupTimeKey = "last_backup_time";
        public void Backup(Action backup_action)
        {
            //int last_backup_time = KeyValueStorage.GetInteger(LastBackupTimeKey);
            KeyValueStorage.SetValue(LastBackupTimeKey, (int)Helpers.UnixTimeStamp());

            backup_action?.Invoke();
        }

        public const string BackupAfterRoutinePreferenceKey = "backup_after_routine";
        public bool TryAskForBackup(Action backup_action)
        {
            const int RoutineResultsBetweenBackups = 5;

            int last_backup_timestamp = KeyValueStorage.GetInteger(LastBackupTimeKey);
            DateTime last_backup_time = Helpers.UnixTimeToDateTime(last_backup_timestamp);
            TimeSpan time_since_last_backup = DateTime.UtcNow - last_backup_time;
            int days_ago = (int)time_since_last_backup.TotalDays;

            var rrs = database.Table<RoutineResult>()
                .Where(rr => !rr.Deleted && rr.StartTime > last_backup_time);

            /*foreach(RoutineResult rr in rrs)
            {
                System.Diagnostics.Debug.WriteLine(rr.StartTime + " " + rr.ToString());
            }*/
            int rr_count_since_last_backup = rrs.Count();

            string pretext = "";
            if(last_backup_timestamp == 0)
            {
                pretext = "You have never backed up your data. ";
            }
            else
            {
                pretext = $"Your last backup was {days_ago} days ago, and you performed" +
                    $" {rr_count_since_last_backup} routines since then. ";
            }

            System.Diagnostics.Debug.WriteLine($"TryAskForBackup rrs={rr_count_since_last_backup} {pretext}");

            if (rr_count_since_last_backup >= RoutineResultsBetweenBackups)
            {
                bool ask = DialogService.KeyValueStorage.GetBoolean(
                    DialogService.AskForKey(BackupAfterRoutinePreferenceKey), true);

                if (ask)
                {
                    DialogService.DisplayConfirmationNeverShowAgain(
                        pretext +
                        "Would you like to perform a backup now?",
                        BackupAfterRoutinePreferenceKey,
                        delegate
                        {
                            Backup(backup_action);
                        });
                }
                
                return ask;
            }
            return false;
        }

        public void GetFreeWeightliftingPrograms(Action navigate_action=null)
        {
            if(navigate_action == null)
            {
                GetFreeWeightliftingProgramsWithNavigationService();
            }
            else
            {
                navigate_action();
            }
        }

        public async Task<Tuple<INavigation, INavigation>> AddPurchaseLicenseNavigation(ICollection<INavigation> navs)
        {
            Navigation purchase_license_nav =
                await GetPurchaseLicenseNavigationLink();
            if (purchase_license_nav != null)
            {
                navs.Add(purchase_license_nav);

                Navigation restore_nav = new Navigation("Restore license", RecheckLicense);

                navs.Add(restore_nav);

                return new Tuple<INavigation, INavigation>(purchase_license_nav, restore_nav);
            }
            return null;
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
                FinishRestore(message);
            }
            else
            {
                MainThreadInvoker.Invoke(delegate
                {
                    FinishRestore(message);
                });
            }
        }

        void FinishRestore(string message)
        {
            if (purchase_navigation != null)
            {
                purchase_navigation.Text = "License purchased";
                purchase_navigation.Click = null;
                ShouldReloadMenu?.Invoke(this, new EventArgs());
            }
            DialogService.DisplayAcknowledgement(message);
        }
    }
}
