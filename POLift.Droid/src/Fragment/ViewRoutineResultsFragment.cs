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

    public class ViewRoutineResultsFragment : ListFragment
    {
        const int EditRoutineResultRequestCode = 5;

        RoutineResultAdapter RoutineResultAdapter;

        IPOLDatabase Database;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            // Create your application here
            Database = C.ontainer.Resolve<IPOLDatabase>();

            RefreshRoutineResults();
        }

        void RefreshRoutineResults()
        {
            RoutineResultAdapter = new RoutineResultAdapter(this.Activity, Database.Table<RoutineResult>()
                .Where(rr => !rr.Deleted)
                .OrderByDescending(rr => rr.EndTime));

            RoutineResultAdapter.DeleteButtonClicked += RoutineResultAdapter_DeleteButtonClicked;
            RoutineResultAdapter.EditButtonClicked += RoutineResultAdapter_EditButtonClicked;

            this.ListAdapter = RoutineResultAdapter;
        }

        

        private void RoutineResultAdapter_EditButtonClicked(object sender, ContainerEventArgs<IRoutineResult> e)
        {
            Intent intent = new Intent(this.Activity, typeof(EditRoutineResultActivity));
            intent.PutExtra("routine_result_id", e.Contents.ID);
            StartActivityForResult(intent, EditRoutineResultRequestCode);
        }

        private void RoutineResultAdapter_DeleteButtonClicked(object sender, ContainerEventArgs<IRoutineResult> e)
        {
            IRoutineResult rr = e.Contents;

            AndroidHelpers.DisplayConfirmation(this.Activity, "Are you sure you want to delete this workout session? " +
                $"{rr.Routine.Name} at {rr.StartTime}",
               delegate
               {
                   DeleteRoutineResult(rr);
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

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            //this.ListView.ItemLongClick += ListView_ItemLongClick;
        }

        /*private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Helpers.DisplayConfirmation(this.Activity, "Do you want to delete this workout session?",
                delegate
                {
                    IRoutineResult to_delete = RoutineResultAdapter[e.Position];
                    RoutineResultAdapter.RoutineResults.RemoveAt(e.Position);

                    Database.HideDeletable((RoutineResult)to_delete);

                    foreach (IExerciseResult ex_r in to_delete.ExerciseResults)
                    {
                        Database.HideDeletable((ExerciseResult)ex_r);
                    }
                });
        }*/


        void DeleteRoutineResult(IRoutineResult to_delete)
        {
            RoutineResultAdapter.RoutineResults.RemoveAll(rr => rr.Equals(to_delete));
            Database.HideDeletable((RoutineResult)to_delete);

            // delete all child ExerciseResults
            foreach (IExerciseResult ex_r in to_delete.ExerciseResults)
            {
                Database.HideDeletable((ExerciseResult)ex_r);
            }
        }
    }
}