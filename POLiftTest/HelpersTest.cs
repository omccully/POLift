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

namespace POLiftTest
{
    [TestFixture]
    public class HelpersTest
    {
        [Test]
        public void CalculatePlateCountsTest()
        {
            float[] imperial_plates = { 2.5f, 5, 10, 25, 45 };

            Dictionary<float, int> result = PlateMath.ImperialBarbellAndPlatesNo35s.CalculateTotalPlateCounts(135);

            Assert.True(result[135f] == 1);
            Assert.True(result.Count() == 1);



        }


    }
}