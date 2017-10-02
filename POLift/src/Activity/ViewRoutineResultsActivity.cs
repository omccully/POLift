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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            this.ListAdapter = new RoutineResultAdapter(this, POLDatabase.Table<RoutineResult>());

        }
    }
}