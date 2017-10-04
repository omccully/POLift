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
using POLift.Service;

using Xamarin.Android.NUnitLite;
using POLift.Model;

namespace POLiftTest
{
    [TestFixture]
    public class HelpersTest
    {
        [TestCase]
        public void CalculatePlateCountsTest()
        {
            float[] imperial_plates = { 2.5f, 5, 10, 25, 45 };

            Dictionary<float, int> result = PlateMath.ImperialBarbellAndPlatesNo35s.CalculateTotalPlateCounts(135);

            Assert.AreEqual(result[45f], 2, "IBAPN35(135) takes " + result[45f] + " 45 lb plates");
            Assert.AreEqual(result.Count(), 1, "IBAPN35(135) does not have 1 element");
        }

        [TestCase]
        public void ToIDStringTest()
        {
            List<Identity> list = new List<Identity>()
            {
                new Identity(5), new Identity(2), new Identity(10)
            };
            Assert.AreEqual(list.ToIDString(), "5,2,10");

            List<Identity> empty_list = new List<Identity>();
            Assert.AreEqual(list.ToIDString(), "");

            List<Identity> one_item_list = new List<Identity>()
            {
                new Identity(7)
            };
            Assert.AreEqual(list.ToIDString(), "7");
        }



    }
}