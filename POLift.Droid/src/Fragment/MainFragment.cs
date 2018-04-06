using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Microsoft.Practices.Unity;

namespace POLift.Droid
{
    using Service;
    using Core.Model;
    using Core.Service;
    using Core.ViewModel;

    [Activity(Label = "Progressive Overload Lifting", MainLauncher = true)]
    class MainFragment : Fragment
    {
        const int CreateRoutineRequestCode = 2;
        const int EditRoutineRequestCode = 3;
        public const int PerformRoutineRequestCode = 484;

        ListView RoutinesList;
        Button CreateRoutineLink;
        RoutineAdapter routine_adapter;
        Button OnTheFlyLink;

        private MainViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.Main;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.Main, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RoutinesList = view.FindViewById<ListView>(Resource.Id.RoutinesList);
            CreateRoutineLink = view.FindViewById<Button>(Resource.Id.CreateRoutineLink);
            OnTheFlyLink = view.FindViewById<Button>(Resource.Id.OnTheFlyLink);

            CreateRoutineLink.Click += CreateRoutineButton_Click;
            OnTheFlyLink.Click += OnTheFlyLink_Click;

            RoutinesList.ItemClick += RoutinesList_ItemClick;
            RoutinesList.ItemLongClick += RoutinesList_ItemLongClick;

            RoutinesList.Focusable = true;
            RoutinesList.Clickable = true;
            RoutinesList.ItemsCanFocus = true;

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this.Activity),
                ViewModelLocator.Default.KeyValueStorage);
            Vm.Toaster = new Toaster(this.Activity);
        }

        private void OnTheFlyLink_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this.Activity, typeof(PerformRoutineActivity));

            intent.PutExtra(PerformRoutineViewModel.OnTheFlyFlagKey, true);

            PerformRoutineActivity.SavedState = null;

            StartActivityForResult(intent, PerformRoutineRequestCode);
        }

        private void RoutinesList_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Intent intent = new Intent(this.Activity, typeof(RoutineDetailsActivity));
            intent.PutExtra("routine_id", routine_adapter.Data[e.Position].Routine.ID);
            StartActivity(intent);
        }

        void RefreshRoutineList()
        {
            routine_adapter = new RoutineAdapter(Activity, Vm.RoutinesList);
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
            Vm.DeleteRoutine(e.Routine, delegate
            {
                RefreshRoutineList();
            });
        }

        private void RoutinesList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var intent = new Intent(Activity, typeof(PerformRoutineActivity));

            IRoutine routine = routine_adapter[e.Position].Routine;

            intent.PutExtra("routine_id", routine.ID);

            PerformRoutineActivity.SavedState = null;

            StartActivityForResult(intent, PerformRoutineRequestCode);
        }

        protected void CreateRoutineButton_Click(object sender, EventArgs e)
        {
            var intent = new Intent(Activity, typeof(CreateRoutineActivity));

            StartActivityForResult(intent, CreateRoutineRequestCode);
        }

        public override void OnResume()
        {
            base.OnResume();

            RefreshRoutineList();
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

               /* int id = data.GetIntExtra("routine_id", -1);
                if (id == -1)
                {
                    return;
                }
                //Routine new_routine = POLDatabase.ReadByID<Routine>(id);

                RefreshRoutineList();*/
            }

            if(requestCode == PerformRoutineRequestCode)
            {
                // routines may have been edited from perform routine page
                // regardless of result
                
                if(resultCode == Result.Ok)
                {
                    Toast.MakeText(this.Activity, "Routine completed",
                        ToastLength.Long).Show();
                }
            }
        }
    }
}