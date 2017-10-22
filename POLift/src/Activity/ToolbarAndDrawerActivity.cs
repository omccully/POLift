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
using Android.Support.V4.Widget;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Support.V4.Content;
using Fragment = Android.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;
using FragmentManager = Android.App.FragmentManager;
using FragmentTransaction = Android.App.FragmentTransaction;

namespace POLift
{
    using Model;

    [Activity(Label = "ToolbarAndDrawerActivity")]
    public class ToolbarAndDrawerActivity : AppCompatActivity
    {
        DrawerLayout _DrawerLayout;
        ListView DrawerListView;
        List<INavigation> Navigations;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Drawer);

            _DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            DrawerListView = FindViewById<ListView>(Resource.Id.left_drawer);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);

            SetSupportActionBar(toolbar);

            Navigations = new List<INavigation>()
            {
                new Navigation("Select routine", SelectRoutine_Click,
                    Resource.Mipmap.ic_fitness_center_white_24dp),
                new Navigation("View recent sessions", ViewRecentSessions_Click,
                    Resource.Mipmap.ic_today_white_24dp),
                new Navigation("View 1RM graphs", View1RMGraphs_Click,
                    Resource.Mipmap.ic_timeline_white_24dp),
                //new Navigation("View gym time graph", ViewGymTimeGraphs_Click,
                //    Resource.Mipmap.ic_timeline_white_24dp),
                new Navigation("Settings", Settings_Click,
                    Resource.Mipmap.ic_settings_white_24dp),
                new Navigation("Help & feedback", HelpAndFeedback_Click,
                    Resource.Mipmap.ic_help_white_24dp),

                new Navigation("Backup data", BackupData_Click,
                    Resource.Mipmap.ic_backup_white_24dp),
                new Navigation("Restore data from backup", RestoreData_Click,
                    Resource.Mipmap.ic_cloud_download_white_24dp)
            };

            DrawerListView.Adapter = new NavigationAdapter(this, Navigations);

            ActionBarDrawerToggle drawer_toggle = new ActionBarDrawerToggle(this, _DrawerLayout, toolbar,
                Resource.String.drawer_opened,
                 Resource.String.drawer_closed);

            _DrawerLayout.SetDrawerListener(drawer_toggle);
            _DrawerLayout.Post(() => drawer_toggle.SyncState());

            DrawerListView.ItemClick += DrawerListView_ItemClick;
        }

        protected void RestoreLastFragment()
        {
            Fragment fragment = FragmentManager.FindFragmentById(Resource.Id.content_frame);

            SwitchToFragment(fragment, false);
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Navigations[e.Position].OnClick();

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

        private void BackupData_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionSend);

            intent.SetType("application/octet-stream");

            //intent.SetType("*/*");

            Java.IO.File export_file = new Java.IO.File(C.DatabasePath);
            Android.Net.Uri uri = FileProvider.GetUriForFile(this,
                "POLift.poliftdatabaseprovider", export_file);

            System.Diagnostics.Debug.WriteLine(export_file.ToString());

            System.Diagnostics.Debug.WriteLine(uri.ToString());

            
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

            // System.Diagnostics.Debug.WriteLine("giving permission for " + chosen.Package);

            // GrantUriPermission(chosen.Package, uri, ActivityFlags.GrantReadUriPermission);

            System.Diagnostics.Debug.WriteLine(intent.Extras.ToString());
            System.Diagnostics.Debug.WriteLine(intent.DataString);
            Intent chosen = Intent.CreateChooser(intent, "Backup via: ");

            System.Diagnostics.Debug.WriteLine(chosen.Extras.ToString());
            intent.PutExtra(Intent.ExtraStream, uri);
            System.Diagnostics.Debug.WriteLine(chosen.Extras.ToString());
            StartActivity(chosen);


            /*//Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
            string dest_path_no_file = this.Activity.GetExternalFilesDir("database").AbsolutePath;
            Android.Net.Uri uri = new POLDatabaseProvider().GetDatabaseURI(this);

            string file_name = Path.GetFileName(C.DatabasePath);
            string dest_path = Path.Combine(dest_path_no_file, file_name);

            System.Diagnostics.Debug.WriteLine(C.DatabasePath);
            System.Diagnostics.Debug.WriteLine(dest_path);
            System.Diagnostics.Debug.WriteLine("***************");
            try
            {
                File.Copy(C.DatabasePath, dest_path);
                System.Diagnostics.Debug.WriteLine("copied");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }*/
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


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == PickFileRequestCode)
            {
                System.Diagnostics.Debug.WriteLine(data.Data);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            // MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnPrepareOptionsMenu(menu);
        }

        public override void OnBackPressed()
        {

            base.OnBackPressed();
        }
    }
}