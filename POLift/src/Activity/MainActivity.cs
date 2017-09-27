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

            List<Routine> routines = new List<Routine>();
            routine_adapter = new RoutineAdapter(this, routines);
            RoutinesList.Adapter = routine_adapter;
        }

        protected void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CreateRoutineActivity));

            StartActivityForResult(intent, CreateRoutineRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(requestCode == CreateRoutineRequestCode)
            {
                string xml = data.GetStringExtra("routine");
                Routine new_routine = Routine.FromXml(xml);

                routine_adapter.Add(new_routine);

            }


        }
    }
}