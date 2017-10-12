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

namespace POLift.Service
{
    interface IPlateMath
    {
        int BarWeight { get; private set; }

        bool SplitWeights { get; private set; }

        Dictionary<float, int> CalculateTotalPlateCounts(int weight);

        string PlateCountsToString(int weight);
    }
}