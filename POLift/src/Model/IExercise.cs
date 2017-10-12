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

namespace POLift.Model
{
    using Service;

    interface IExercise : IIdentifiable, IDeletable, IDatabaseObject
    {
        string Name { get; set; }

        int MaxRepCount { get; set; }

        int WeightIncrement { get; set; }

        int RestPeriodSeconds { get; set; }

        int PlateMathID { get; set; }

        string Category { get; set; }

        PlateMath PlateMath { get; set; }

        string ShortDetails { get; set; }

        int NextWeight { get; set; }
    }
}