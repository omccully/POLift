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

using Microsoft.Practices.Unity;

namespace POLift
{
    using Model;
    using Service;

    [Activity(Label = "Past lifting sessions")]
    public class ViewRoutineResultsActivity : ListActivity
    {
        RoutineResultAdapter RoutineResultAdapter;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            Database = C.ontainer.Resolve<IPOLDatabase>();

            RoutineResultAdapter = new RoutineResultAdapter(this, Database.Table<RoutineResult>()
                .Where(rr => !rr.Deleted));
            this.ListAdapter = RoutineResultAdapter;

            this.ListView.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Helpers.DisplayConfirmation(this, "Do you want to delete this workout session?",
                delegate
                {
                    IRoutineResult to_delete = RoutineResultAdapter[e.Position];
                    RoutineResultAdapter.RoutineResults.RemoveAt(e.Position);

                    Database.HideDeletable((RoutineResult)to_delete);

                    foreach(IExerciseResult ex_r in to_delete.ExerciseResults)
                    {
                        Database.HideDeletable((ExerciseResult)ex_r);
                    }
                });
        }
    }
}