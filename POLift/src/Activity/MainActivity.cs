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

    [Activity(Label = "Progressive Overload Lifting", MainLauncher = true)]
    class MainActivity : ToolbarAndDrawerActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
           
            if(savedInstanceState == null)
            {
                SwitchToFragment(new MainFragment(), false);
            }
            else
            {
                RestoreLastFragment();
            }
        }
    }
}