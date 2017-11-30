using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Android.Util;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Preferences;
using Fragment = Android.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using FragmentManager = Android.App.FragmentManager;
using FragmentTransaction = Android.App.FragmentTransaction;

using Microsoft.Practices.Unity;

namespace POLift
{
    using Service;
    using Core.Model;
    using Core.Service;

    [Activity(Label = "ToolbarAndDrawerActivity")]
    public class ToolbarAndDrawerActivity : AppCompatActivity
    {
        const int GetFreeLiftingProgramsRequestCode = 54392;

        DrawerLayout _DrawerLayout;
        ListView DrawerListView;
        NavigationAdapter _NavigationAdapter;

        protected IPOLDatabase Database;
        protected ILicenseManager LicenseManager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Drawer);

            Database = C.ontainer.Resolve<IPOLDatabase>();
            LicenseManager = C.ontainer.Resolve<ILicenseManager>();

            _DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            DrawerListView = FindViewById<ListView>(Resource.Id.left_drawer);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            List<INavigation> Navigations = new List<INavigation>()
            {
                new Navigation("Select routine", SelectRoutine_Click,
                    Resource.Mipmap.ic_fitness_center_white_24dp),
                new Navigation("View recent sessions", ViewRecentSessions_Click,
                    Resource.Mipmap.ic_today_white_24dp),
                new Navigation("View 1RM graphs", View1RMGraphs_Click,
                    Resource.Mipmap.ic_timeline_white_24dp),
                //new Navigation("View gym time graph", ViewGymTimeGraphs_Click,
                //    Resource.Mipmap.ic_timeline_white_24dp),
                
                new Navigation("Backup data", BackupData_Click,
                    Resource.Mipmap.ic_backup_white_24dp),
                new Navigation("Import data from backup", RestoreData_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),
                new Navigation("Import routines and exercises only", ImportRoutinesAndExercises_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),
                new Navigation("Get free lifting programs", GetFreeLiftingPrograms_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp),

                new Navigation("Settings", Settings_Click,
                    Resource.Mipmap.ic_settings_white_24dp),
                new Navigation("Help & feedback", HelpAndFeedback_Click,
                    Resource.Mipmap.ic_help_white_24dp)

                    /*,
                     * TODO: export data as text
                new Navigation("Export data as text", ExportAsText_Click,
                    Resource.Mipmap.ic_backup_white_24dp)*/
            };

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            if (!prefs.GetBoolean("has_rated_app", false))
            {
                Navigations.Add(new Navigation("Rate app",
                    RateApp_Click, Resource.Mipmap.ic_rate_review_white_18dp));
            }

#if DEBUG
            //Navigations.Add(new Navigation("Metricize", Metricize_Click,
            //    Resource.Mipmap.ic_settings_white_24dp));
#endif

            _NavigationAdapter = new NavigationAdapter(this, Navigations);

            DrawerListView.Adapter = _NavigationAdapter;

            ActionBarDrawerToggle drawer_toggle = new ActionBarDrawerToggle(this, _DrawerLayout, toolbar,
                Resource.String.drawer_opened,
                 Resource.String.drawer_closed);
            
            _DrawerLayout.SetDrawerListener(drawer_toggle);
            _DrawerLayout.Post(() => drawer_toggle.SyncState());

            DrawerListView.ItemClick += DrawerListView_ItemClick;

            PromptUserForExternalProgramsIfFirstLaunch();

            AddPurchaseLicenseNavigationIfNotPurchased();
        }

        void PromptUserForExternalProgramsIfFirstLaunch()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            bool first_launch = 
                prefs.GetBoolean("first_launch_for_external_programs", true);
            Log.Debug("POLift", $"first_launch_for_external_programs = {first_launch}");

            if(first_launch)
            {
                AndroidHelpers.DisplayConfirmation(this, 
                    "You can get started with the app right away " +
                    "by using one of the built-in weightlifting programs. " +
                    "They focus on compound lifts that have withstood " +
                    "the test of time. Would you like to select a " +
                    "built-in program now?",
                    delegate
                    {
                        FlagExternalProgramsResponse(prefs);
                        GetFreeLiftingPrograms();
                    },
                    delegate
                    {
                        FlagExternalProgramsResponse(prefs);
                        Toast.MakeText(this, "If you change your mind, you " +
                            "can get one of the built-in working programs " +
                            "at any time from the navigation drawer by swiping " +
                            "from the left.", ToastLength.Long).Show();
                    });
            }
        }

        void FlagExternalProgramsResponse(ISharedPreferences prefs)
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("first_launch_for_external_programs", false);
            editor.Apply();
        }

        async Task AddPurchaseLicenseNavigationIfNotPurchased()
        {
            if (!(await LicenseManager.CheckLicense(false)))
            {
                // could hide this button until like 14 days are left

                Navigation LicenseNavigation = 
                    new Navigation("Purchase lifetime license",
                    PurchaseLicense_Click, 
                    Resource.Mipmap.ic_shopping_basket_white_24dp);

                _NavigationAdapter.Navigations.Add(LicenseNavigation);

                PutDaysLeftAtEndOfNavigation(LicenseNavigation);
            }
        }

        async Task PutDaysLeftAtEndOfNavigation(Navigation navigation)
        {
            try
            {
                int sec_left = await LicenseManager.SecondsRemainingInTrial();
                int days_left = Math.Abs(sec_left / 86400);
                string plur = days_left == 1 ? "" : "s";

                if (sec_left > 0)
                {
                    navigation.Text += $" ({days_left} day{plur} left)";
                }
                else
                {
                    navigation.Text += $" (expired {days_left} day{plur} ago)";
                }

                _NavigationAdapter.NotifyDataSetChanged();
            }
            catch
            {

            }
        }

        protected void RestoreLastFragment()
        {
            Fragment fragment = FragmentManager.FindFragmentById(Resource.Id.content_frame);

            SwitchToFragment(fragment, false);
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            _NavigationAdapter.Navigations[e.Position].OnClick();

            //Navigations[e.Position].OnClick();

            _DrawerLayout.CloseDrawers();
        }

        protected void SwitchToFragment(Fragment fragment, bool add_to_backstack = true)
        {
            FragmentTransaction transaction = FragmentManager.BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment);

            if (add_to_backstack)
            {
                transaction.AddToBackStack(fragment.GetType().ToString());
            }

            transaction.Commit();
        }

        private void ViewRecentSessions_Click(object sender, EventArgs e)
        {
            SwitchToFragment(new ViewRoutineResultsFragment());
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            SwitchToFragment(new SettingsFragment());
        }

        private void View1RMGraphs_Click(object sender, EventArgs e)
        {
            SwitchToFragment(new GraphFragment());
        }

        private void ViewGymTimeGraphs_Click(object sender, EventArgs e)
        {

        }

        private void SelectRoutine_Click(object sender, EventArgs e)
        {
            SwitchToFragment(new MainFragment());
        }

        private void HelpAndFeedback_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse("https://reddit.com/r/POLift/"));
            StartActivity(intent);
        }

        private void GetFreeLiftingPrograms_Click(object sender, EventArgs e)
        {
            GetFreeLiftingPrograms();
        }

        private void RateApp_Click(object sender, EventArgs e)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            StartActivity(new Intent(Intent.ActionView,
                Android.Net.Uri.Parse("market://details?id=com.cml.polift")));
            prefs.Edit().PutBoolean("has_rated_app", true).Apply();
        }

        private void Metricize_Click(object sender, EventArgs e)
        {
            AndroidHelpers.DisplayConfirmation(this,
                "Are you sure you want to convert exercises to metric? " +
                "If yes, make sure you do a backup first.",
                delegate
                {
                    ConvertEverythingToMetric();
                });
        }

        void ConvertEverythingToMetric()
        {
            foreach(Exercise ex in Database.Table<Exercise>())
            {
                ex.WeightIncrement /= 2;

                IPlateMath pm = ex.PlateMath;

                if(pm == null)
                {

                }
                else if(pm.BarWeight == 45)
                {
                    ex.PlateMath = PlateMath.MetricBarbellAndPlates;
                }
                else
                {
                    if(pm.BarWeight == 0)
                    {
                        // no bar
                        if (pm.SplitWeights)
                        {
                            ex.PlateMath = PlateMath.MetricPlates;
                        }
                        else
                        {
                            ex.PlateMath = 
                                PlateMath.MetricPlatesNoSplit;
                        }
                    }
                }

                Database.Update(ex);
            }

            foreach(Routine r in Database.Table<Routine>())
            {
                if(r.Name.Contains("imperial"))
                {
                    r.Name = r.Name.Replace("imperial", "metric");

                    Database.Update(r);
                }
            }

            ExercisesChanged();
        }
        
        void GetFreeLiftingPrograms()
        {
            StartActivityForResult(typeof(SelectProgramToDownloadActivity), 
                GetFreeLiftingProgramsRequestCode);
        }

        private void BackupData_Click(object sender, EventArgs e)
        {
            Java.IO.File export_file = new Java.IO.File(C.DatabasePath);
            Android.Net.Uri uri = FileProvider.GetUriForFile(this,
                "com.cml.poliftprovider", export_file);

            Intent share_intent = ShareCompat.IntentBuilder.From(this)
                .SetType("application/octet-stream")
                .SetStream(uri)
                .Intent;

            share_intent.SetData(uri);
            share_intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            StartActivity(share_intent);


            /*Intent intent = new Intent(Intent.ActionSend);

            intent.SetType("application/octet-stream");

            intent.PutExtra(Intent.ExtraStream, uri);

            //intent.SetData(uri);
            //intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            // grant permission to all
            IList<ResolveInfo> res_info_list = PackageManager.QueryIntentActivities(intent,
                PackageInfoFlags.MatchDefaultOnly);
            foreach (ResolveInfo res_info in res_info_list)
            {
                string package_name = res_info.ActivityInfo.PackageName;
                System.Diagnostics.Debug.WriteLine("giving permission for " + package_name);
                GrantUriPermission(package_name, uri, ActivityFlags.GrantReadUriPermission);
            }

            StartActivity(Intent.CreateChooser(intent, "Backup via: "));*/
        }


        const int PickFileRequestCode = 12311;
        private void RestoreData_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            //intent.SetType("application/octet-stream");
            intent.SetType("*/*");
            //intent.PutExtra("CONTENT_TYPE", "*/*");
            StartActivityForResult(intent, PickFileRequestCode);

        }

        const int PickFileForRoutineAndExerciseImportCode = 543261;
        private void ImportRoutinesAndExercises_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionGetContent);
            //intent.SetType("application/octet-stream");
            intent.SetType("*/*");
            //intent.PutExtra("CONTENT_TYPE", "*/*");
            StartActivityForResult(intent, PickFileForRoutineAndExerciseImportCode);

        }
        

        private void PurchaseLicense_Click(object sender, EventArgs e)
        {
            PurchaseLicense();
        }

        async void PurchaseLicense()
        {
            bool bought = await LicenseManager.PromptToBuyLicense();

            System.Diagnostics.Debug.WriteLine($"bought = {bought}");
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok)
            { 
                if(requestCode == PickFileRequestCode)
                {
                    AndroidHelpers.DisplayConfirmation(this,
                       "Are you sure you want to import this file (" +
                       Path.GetFileName(data.Data.Path) + ")? " +
                       "You may want to perform a backup of your current " +
                       "data in case something unintended happens.", delegate
                       {
                           ImportFromUri(data.Data);
                       });
                }
                else if(requestCode == PickFileForRoutineAndExerciseImportCode)
                {
                    AndroidHelpers.DisplayConfirmation(this,
                       "Are you sure you want to import the routines and exercises from this file (" +
                       Path.GetFileName(data.Data.Path) + ")? " +
                       "You may want to perform a backup of your current " +
                       "data in case something unintended happens.", delegate
                       {
                           ImportFromUri(data.Data, false);
                       });
                }
                else if(requestCode == GetFreeLiftingProgramsRequestCode)
                {
                    GoToMainFragment();
                }
            }
        }

        void ExercisesChanged()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("exercise_created_since_last_difficulty_regeneration", true);
            editor.Apply();
        }

        void ImportFromUri(Android.Net.Uri uri, bool full = true)
        {
            AndroidHelpers.ImportFromUri(uri, Database, this.ContentResolver, 
                FilesDir.Path, new FileOperations(), full);

            GoToMainFragment();
        }

        void GoToMainFragment()
        {
            ExercisesChanged();

            SwitchToFragment(new MainFragment(), false);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override void OnBackPressed()
        {
            if (this._DrawerLayout.IsDrawerOpen((int)GravityFlags.Start))
            {
                this._DrawerLayout.CloseDrawer((int)GravityFlags.Start);
            }
            else
            {
                base.OnBackPressed();
            }
            
        }
    }
}