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
    [Register ("CreateRoutineController")]
    partial class CreateRoutineController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton AddExerciseLink { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CreateRoutineButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ExerciseSetsTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField RoutineNameTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TipLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AddExerciseLink != null) {
                AddExerciseLink.Dispose ();
                AddExerciseLink = null;
            }

            if (CreateRoutineButton != null) {
                CreateRoutineButton.Dispose ();
                CreateRoutineButton = null;
            }

            if (ExerciseSetsTableView != null) {
                ExerciseSetsTableView.Dispose ();
                ExerciseSetsTableView = null;
            }

            if (RoutineNameTextField != null) {
                RoutineNameTextField.Dispose ();
                RoutineNameTextField = null;
            }

            if (TipLabel != null) {
                TipLabel.Dispose ();
                TipLabel = null;
            }
        }
    }
}