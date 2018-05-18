using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Microsoft.Practices.Unity;

namespace POLift.Droid
{
    using Service;
    using Core.Service;
    using Core.Model;
    using Core.ViewModel;

    public class ViewRoutineResultsFragment : ListFragment
    {
        const int EditRoutineResultRequestCode = 5;

        RoutineResultAdapter RoutineResultAdapter;

        private ViewRoutineResultsViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.ViewRoutineResults;
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this.Activity),
                ViewModelLocator.Default.KeyValueStorage);

            RefreshRoutineResults();
        }

        void RefreshRoutineResults()
        {
            RoutineResultAdapter = new RoutineResultAdapter(this.Activity, 
                Vm.RoutineResults);

            RoutineResultAdapter.DeleteButtonClicked += RoutineResultAdapter_DeleteButtonClicked;
            RoutineResultAdapter.EditButtonClicked += RoutineResultAdapter_EditButtonClicked;
            RoutineResultAdapter.ShareButtonClicked += RoutineResultAdapter_ShareButtonClicked;

            this.ListAdapter = RoutineResultAdapter;
        }

        private void RoutineResultAdapter_ShareButtonClicked(object sender, ContainerEventArgs<IRoutineResult> e)
        {
            this.Activity.ShareRoutineResult(e.Contents);
        }

        private void RoutineResultAdapter_EditButtonClicked(object sender, ContainerEventArgs<IRoutineResult> e)
        {
            Intent intent = new Intent(this.Activity, typeof(EditRoutineResultActivity));
            intent.PutExtra("routine_result_id", e.Contents.ID);
            StartActivityForResult(intent, EditRoutineResultRequestCode);
        }

        private void RoutineResultAdapter_DeleteButtonClicked(object sender, ContainerEventArgs<IRoutineResult> e)
        {
            IRoutineResult to_delete = e.Contents;
            Vm.DeleteRoutineResult(to_delete, delegate
            {
                RoutineResultAdapter.RoutineResults.RemoveAll(rr => rr.Equals(to_delete));
            });
        }

        public override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == EditRoutineResultRequestCode)
            {
                RefreshRoutineResults();
            }
        }
    }
}