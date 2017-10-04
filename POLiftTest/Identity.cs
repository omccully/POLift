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

using POLift.Model;

namespace POLiftTest
{
    class Identity : IIdentifiable
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public Identity(int id, string Name = "no name")
        {
            this.ID = id;
            this.Name = Name;
        }
    }
}