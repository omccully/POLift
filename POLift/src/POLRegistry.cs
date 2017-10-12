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

using StructureMap;
using StructureMap.Graph;

namespace POLift
{
    using Model;
    using Service;

    class POLRegistry : Registry
    {
        public POLRegistry()
        {
            //this.For<>
            Container c = new Container(x =>
            {
                x.For<IPOLDatabase>().Use<POLDatabase>().Singleton();
            });
        }
    }
}