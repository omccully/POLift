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

namespace POLift.iOS.Controllers
{
    [Register ("PerformWarmupController")]
    partial class PerformWarmupController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ExerciseDetailsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RoutineDetailsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SetCompletedButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView TimerContainer { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField WeightTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ExerciseDetailsLabel != null) {
                ExerciseDetailsLabel.Dispose ();
                ExerciseDetailsLabel = null;
            }

            if (RoutineDetailsLabel != null) {
                RoutineDetailsLabel.Dispose ();
                RoutineDetailsLabel = null;
            }

            if (SetCompletedButton != null) {
                SetCompletedButton.Dispose ();
                SetCompletedButton = null;
            }

            if (TimerContainer != null) {
                TimerContainer.Dispose ();
                TimerContainer = null;
            }

            if (WeightTextField != null) {
                WeightTextField.Dispose ();
                WeightTextField = null;
            }
        }
    }
}