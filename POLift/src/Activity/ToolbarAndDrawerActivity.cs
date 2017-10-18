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
using Fragment = Android.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;

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
                new Navigation("View recent sessions", ViewRecentSessions_Click,
                    Resource.Mipmap.ic_today_white_24dp),
                new Navigation("View 1RM graphs", View1RMGraphs_Click,
                    Resource.Mipmap.ic_timeline_white_24dp),
                new Navigation("View gym time graph", ViewGymTimeGraphs_Click,
                    Resource.Mipmap.ic_timeline_white_24dp),
                new Navigation("Settings", Settings_Click,
                    Resource.Mipmap.ic_settings_white_24dp),
                new Navigation("Help & feedback", HelpAndFeedback_Click,
                    Resource.Mipmap.ic_help_white_24dp)
            };

            DrawerListView.Adapter = new NavigationAdapter(this, Navigations);

            ActionBarDrawerToggle drawer_toggle = new ActionBarDrawerToggle(this, _DrawerLayout, toolbar,
                Resource.String.drawer_opened,
                 Resource.String.drawer_closed);
            
            _DrawerLayout.SetDrawerListener(drawer_toggle);
            _DrawerLayout.Post(() => drawer_toggle.SyncState());

            DrawerListView.ItemClick += DrawerListView_ItemClick;
        }

        private void DrawerListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Navigations[e.Position].OnClick();

            _DrawerLayout.CloseDrawers();
        }

        protected void SwitchToFragment(Fragment fragment)
        {
            // Replace(Resource.Id.content_frame, fragment)

            FragmentManager.BeginTransaction()
                //.Add(Resource.Id.content_frame, fragment)
                .Replace(Resource.Id.content_frame, fragment)
                .AddToBackStack(null)
                .Commit();
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

        private void HelpAndFeedback_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse("https://reddit.com/r/CrystalMathLabs"));
            StartActivity(intent);
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