// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace POLift.iOS
{
    [Register ("RoutineResultCell")]
    partial class RoutineResultCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EditRoutineResultButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RoutineResultLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (EditRoutineResultButton != null) {
                EditRoutineResultButton.Dispose ();
                EditRoutineResultButton = null;
            }

            if (RoutineResultLabel != null) {
                RoutineResultLabel.Dispose ();
                RoutineResultLabel = null;
            }
        }
    }
}