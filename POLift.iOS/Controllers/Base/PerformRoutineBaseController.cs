using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Model;
using POLift.Core.Service;

namespace POLift.iOS.Controllers
{
    public class PerformRoutineBaseController : UIViewController
    {
        protected IExercise CurrentExercise;

        protected IPlateMath CurrentPlateMath
        {
            get
            {
                if (CurrentExercise == null) return null;
                return PlateMath.PlateMathTypes[CurrentExercise.PlateMathID];
            }
        }

        public PerformRoutineBaseController(IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            
        }

    }
}