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
    [Register ("ExerciseSetsCell")]
    partial class ExerciseSetsCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ExerciseNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField SetCountTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ExerciseNameLabel != null) {
                ExerciseNameLabel.Dispose ();
                ExerciseNameLabel = null;
            }

            if (SetCountTextField != null) {
                SetCountTextField.Dispose ();
                SetCountTextField = null;
            }
        }
    }
}