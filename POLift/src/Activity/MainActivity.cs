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

namespace POLift
{
    using Model;
    using Service;

    [Activity(Label = "Progressive Overload Lifting", MainLauncher = true)]
    class MainActivity : Activity
    {
        const int CreateRoutineRequestCode = 2;
        const int EditRoutineRequestCode = 3;

        ListView RoutinesList;
        Button CreateRoutineLink;
        Button ViewRecentSessionsLink;

        RoutineAdapter routine_adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            RoutinesList = FindViewById<ListView>(Resource.Id.RoutinesList);
            ViewRecentSessionsLink = FindViewById<Button>(Resource.Id.ViewRecentSessionsButton);
            ViewRecentSessionsLink.Click += ViewRecentSessionsLink_Click;

            CreateRoutineLink = FindViewById<Button>(Resource.Id.CreateRoutineLink);
            CreateRoutineLink.Click += CreateRoutineButton_Click;

            RoutinesList.ItemClick += RoutinesList_ItemClick;

            RoutinesList.Focusable = true;
            RoutinesList.Clickable = true;
            RoutinesList.ItemsCanFocus = true;

            RefreshRoutineList();
        }

        private void ViewRecentSessionsLink_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(ViewRoutineResultsActivity));
            StartActivity(intent);
        }

        void RefreshRoutineList()
        {
            routine_adapter = new RoutineAdapter(this,
                POLDatabase.TableWhereUndeleted<Routine>());
            RoutinesList.Adapter = routine_adapter;

            routine_adapter.DeleteButtonClicked += Routine_adapter_DeleteButtonClicked;
            routine_adapter.EditButtonClicked += Routine_adapter_EditButtonClicked;
        }

        private void Routine_adapter_EditButtonClicked(object sender, RoutineEventArgs e)
        {
            var intent = new Intent(this, typeof(CreateRoutineActivity));
            intent.PutExtra("edit_routine_id", e.Routine.ID);
            StartActivityForResult(intent, EditRoutineRequestCode);
        }

        private void Routine_adapter_DeleteButtonClicked(object sender, RoutineEventArgs e)
        {
            Helpers.DisplayConfirmation(this, "Are you sure you want to delete this routine?",
                delegate
                {
                    // yes
                    Routine routine_to_delete = e.Routine;

                    routine_to_delete.Deleted = true;

                    POLDatabase.Update(routine_to_delete);

                    RefreshRoutineList();
                });
        }

        private void RoutinesList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(this, typeof(PerformRoutineActivity));

            Routine routine = routine_adapter[e.Position];
            int id = routine.ID;

            intent.PutExtra("routine_id", id);
    
            StartActivity(intent);
        }

        protected void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CreateRoutineActivity));

            StartActivityForResult(intent, CreateRoutineRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && 
                (requestCode == CreateRoutineRequestCode || 
                requestCode == EditRoutineRequestCode))
            {
                //string xml = data.GetStringExtra("routine");
                //Routine new_routine = Routine.FromXml(xml);

                int id = data.GetIntExtra("routine_id", -1);
                if (id == -1)
                {
                    return;
                }
                Routine new_routine = POLDatabase.ReadByID<Routine>(id);

                RefreshRoutineList();

            }
        }
    }
}