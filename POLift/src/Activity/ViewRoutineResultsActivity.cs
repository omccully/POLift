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

    [Activity(Label = "Past lifting sessions")]
    public class ViewRoutineResultsActivity : ListActivity
    {
        RoutineResultAdapter RoutineResultAdapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            RoutineResultAdapter = new RoutineResultAdapter(this, POLDatabase.Table<RoutineResult>()
                .Where(rr => !rr.Deleted));
            this.ListAdapter = RoutineResultAdapter;

            this.ListView.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            Helpers.DisplayConfirmation(this, "Do you want to delete this workout session?",
                delegate
                {
                    RoutineResult to_delete = RoutineResultAdapter[e.Position];
                    RoutineResultAdapter.RemoveIndex(e.Position);

                    POLDatabase.HideDeletable(to_delete);

                    RoutineResultAdapter.NotifyDataSetChanged();
                });
        }
    }
}