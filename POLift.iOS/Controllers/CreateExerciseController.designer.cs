﻿// WARNING
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
    [Register ("CreateExerciseController")]
    partial class CreateExerciseController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ConsecutiveSetsTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CreateExerciseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField ExerciseNameTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView MathTypePicker { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField RepCountTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField RestPeriodTextField { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField WeightIncrementTextField { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ConsecutiveSetsTextField != null) {
                ConsecutiveSetsTextField.Dispose ();
                ConsecutiveSetsTextField = null;
            }

            if (CreateExerciseButton != null) {
                CreateExerciseButton.Dispose ();
                CreateExerciseButton = null;
            }

            if (ExerciseNameTextField != null) {
                ExerciseNameTextField.Dispose ();
                ExerciseNameTextField = null;
            }

            if (MathTypePicker != null) {
                MathTypePicker.Dispose ();
                MathTypePicker = null;
            }

            if (RepCountTextField != null) {
                RepCountTextField.Dispose ();
                RepCountTextField = null;
            }

            if (RestPeriodTextField != null) {
                RestPeriodTextField.Dispose ();
                RestPeriodTextField = null;
            }

            if (WeightIncrementTextField != null) {
                WeightIncrementTextField.Dispose ();
                WeightIncrementTextField = null;
            }
        }
    }
}