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
    [Register ("RoutineCell")]
    partial class RoutineCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EditRoutinebutton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RoutineLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (EditRoutinebutton != null) {
                EditRoutinebutton.Dispose ();
                EditRoutinebutton = null;
            }

            if (RoutineLabel != null) {
                RoutineLabel.Dispose ();
                RoutineLabel = null;
            }
        }
    }
}