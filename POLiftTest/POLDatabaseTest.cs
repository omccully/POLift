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
using NUnit.Framework;
using Xamarin.Android.NUnitLite;

using POLift.Service;

namespace POLiftTest
{
    public class POLDatabaseTest
    {
        [SetUp]
        public void Setup()
        {
            POLDatabase.ClearDatabase();
        }

        [Test]
        public void InsertOrUpdateByID()
        {

        }

    }
}