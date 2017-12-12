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
    [Register ("SelectExerciseController")]
    partial class SelectExerciseController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CreateExerciseLink { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ExerciseListTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CreateExerciseLink != null) {
                CreateExerciseLink.Dispose ();
                CreateExerciseLink = null;
            }

            if (ExerciseListTableView != null) {
                ExerciseListTableView.Dispose ();
                ExerciseListTableView = null;
            }
        }
    }
}