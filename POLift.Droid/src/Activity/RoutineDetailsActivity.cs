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

namespace POLift.Droid
{
    using Core.Service;
    using Core.Model;

    [Activity(Label = "RoutineDetailsActivity")]
    public class RoutineDetailsActivity : Activity
    {
        IPOLDatabase Database;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.RoutineDetails);
            Database = C.Database;

            TextView DetailsTextView = FindViewById<TextView>(
                Resource.Id.RoutineDetailsTextView);

            int routine_id = Intent.GetIntExtra("routine_id", -1);

            Routine routine = Database.ReadByID<Routine>(routine_id);

            if(routine == null)
            {
                DetailsTextView.Text = "error";
            }
            else
            {
                DetailsTextView.Text = routine.ExtendedDetails;
            }
        }
    }
}