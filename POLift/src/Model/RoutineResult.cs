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

using SQLite;

namespace POLift.Model
{
    class RoutineResult : IIdentifiable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int RoutineID { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public RoutineResult(Routine Routine)
        {
            this.RoutineID = Routine.ID;
        }

        public RoutineResult(int RoutineID)
        {
            this.RoutineID = RoutineID;
        }

    }
}