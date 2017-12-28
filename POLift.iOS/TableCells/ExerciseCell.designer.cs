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
    [Register ("ExerciseCell")]
    partial class ExerciseCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EditExerciseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ExerciseLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (EditExerciseButton != null) {
                EditExerciseButton.Dispose ();
                EditExerciseButton = null;
            }

            if (ExerciseLabel != null) {
                ExerciseLabel.Dispose ();
                ExerciseLabel = null;
            }
        }
    }
}