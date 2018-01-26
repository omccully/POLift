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
    [Register ("PerformRoutineController")]
    partial class PerformRoutineController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView Container { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton EditCurrentExerciseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ExerciseDetailsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton IMadeAMistakeButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ModifyRestOfRoutineButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel PlateMathLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField RepCountTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RepDetailsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ReportResultButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel RoutineDetailsLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField WeightTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (Container != null) {
                Container.Dispose ();
                Container = null;
            }

            if (EditCurrentExerciseButton != null) {
                EditCurrentExerciseButton.Dispose ();
                EditCurrentExerciseButton = null;
            }

            if (ExerciseDetailsLabel != null) {
                ExerciseDetailsLabel.Dispose ();
                ExerciseDetailsLabel = null;
            }

            if (IMadeAMistakeButton != null) {
                IMadeAMistakeButton.Dispose ();
                IMadeAMistakeButton = null;
            }

            if (ModifyRestOfRoutineButton != null) {
                ModifyRestOfRoutineButton.Dispose ();
                ModifyRestOfRoutineButton = null;
            }

            if (PlateMathLabel != null) {
                PlateMathLabel.Dispose ();
                PlateMathLabel = null;
            }

            if (RepCountTextField != null) {
                RepCountTextField.Dispose ();
                RepCountTextField = null;
            }

            if (RepDetailsLabel != null) {
                RepDetailsLabel.Dispose ();
                RepDetailsLabel = null;
            }

            if (ReportResultButton != null) {
                ReportResultButton.Dispose ();
                ReportResultButton = null;
            }

            if (RoutineDetailsLabel != null) {
                RoutineDetailsLabel.Dispose ();
                RoutineDetailsLabel = null;
            }

            if (WeightTextField != null) {
                WeightTextField.Dispose ();
                WeightTextField = null;
            }
        }
    }
}