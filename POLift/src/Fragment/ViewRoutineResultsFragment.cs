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

namespace POLift
{
    using Service;
    using Model;

    public class ViewRoutineResultsFragment : ListFragment
    {
        RoutineResultAdapter RoutineResultAdapter;

        IPOLDatabase Database;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            // Create your application here
            Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutineResultAdapter = new RoutineResultAdapter(this.Activity, Database.Table<RoutineResult>()
                .Where(rr => !rr.Deleted).OrderBy(rr => rr.EndTime));
            this.ListAdapter = RoutineResultAdapter;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            this.ListView.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
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
        }
    }
}