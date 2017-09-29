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
        static int CreateRoutineRequestCode = 2;

        ListView RoutinesList;
        Button CreateRoutineLink;

        RoutineAdapter routine_adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Main);

            RoutinesList = FindViewById<ListView>(Resource.Id.RoutinesList);

            CreateRoutineLink = FindViewById<Button>(Resource.Id.CreateRoutineLink);
            CreateRoutineLink.Click += CreateRoutineButton_Click;

            RoutinesList.ItemClick += RoutinesList_ItemClick;
            
            routine_adapter = new RoutineAdapter(this, POLDatabase.Table<Routine>());
            RoutinesList.Adapter = routine_adapter;
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

            if(resultCode == Result.Ok && requestCode == CreateRoutineRequestCode)
            {
                //string xml = data.GetStringExtra("routine");
                //Routine new_routine = Routine.FromXml(xml);

                int id = data.GetIntExtra("routine_id", -1);
                if (id == -1) return;
                Routine new_routine = POLDatabase.ReadByID<Routine>(id);

                routine_adapter.Add(new_routine);

            }


        }
    }
}