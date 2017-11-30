﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Practices.Unity;

namespace POLift
{
    using Service;
    using Core.Model;
    using Core.Service;

    [Activity(Label = "Progressive Overload Lifting", MainLauncher = true)]
    class MainFragment : Fragment
    {
        const int CreateRoutineRequestCode = 2;
        const int EditRoutineRequestCode = 3;
        const int PerformRoutineRequestCode = 4;

        ListView RoutinesList;
        Button CreateRoutineLink;
        RoutineAdapter routine_adapter;

        IPOLDatabase Database;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            return inflater.Inflate(Resource.Layout.Main, container, false);

        }

        

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //SetContentView(Resource.Layout.Main);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutinesList = view.FindViewById<ListView>(Resource.Id.RoutinesList);

            CreateRoutineLink = view.FindViewById<Button>(Resource.Id.CreateRoutineLink);
            CreateRoutineLink.Click += CreateRoutineButton_Click;

            RoutinesList.ItemClick += RoutinesList_ItemClick;
            RoutinesList.ItemLongClick += RoutinesList_ItemLongClick;

            RoutinesList.Focusable = true;
            RoutinesList.Clickable = true;
            RoutinesList.ItemsCanFocus = true;

            RefreshRoutineList();
        }

        private void RoutinesList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Intent intent = new Intent(this.Activity, typeof(RoutineDetailsActivity));
            intent.PutExtra("routine_id", routine_adapter.Routines[e.Position].ID);
            StartActivity(intent);
        }

        void RefreshRoutineList()
        {
            routine_adapter = new RoutineAdapter(Activity,
                Helpers.MainPageRoutinesList(Database));
            RoutinesList.Adapter = routine_adapter;

            routine_adapter.DeleteButtonClicked += Routine_adapter_DeleteButtonClicked;
            routine_adapter.EditButtonClicked += Routine_adapter_EditButtonClicked;
        }

        private void Routine_adapter_EditButtonClicked(object sender, RoutineEventArgs e)
        {
            var intent = new Intent(Activity, typeof(CreateRoutineActivity));
            intent.PutExtra("edit_routine_id", e.Routine.ID);
            StartActivityForResult(intent, EditRoutineRequestCode);
        }

        private void Routine_adapter_DeleteButtonClicked(object sender, RoutineEventArgs e)
        {
            AndroidHelpers.DisplayConfirmation(Activity, "Are you sure you want to delete this routine?",
                delegate
                {
                    // yes
                    IRoutine routine_to_delete = e.Routine;

                    routine_to_delete.Deleted = true;

                    Database.Update(routine_to_delete);

                    RefreshRoutineList();
                });
        }

        private void RoutinesList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(Activity, typeof(PerformRoutineActivity));

            IRoutine routine = routine_adapter[e.Position];
            int id = routine.ID;

            intent.PutExtra("routine_id", id);
    
            StartActivityForResult(intent, PerformRoutineRequestCode);
        }

        protected void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(CreateRoutineActivity));

            StartActivityForResult(intent, CreateRoutineRequestCode);
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
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
                //Routine new_routine = POLDatabase.ReadByID<Routine>(id);

                RefreshRoutineList();
            }

            if(requestCode == PerformRoutineRequestCode)
            {
                // routines may have been edited from perform routine page
                // regardless of result
                RefreshRoutineList();

                if(resultCode == Result.Ok)
                {
                    Toast.MakeText(this.Activity, "Routine completed",
                        ToastLength.Long).Show();
                }
            }
        }
    }
}