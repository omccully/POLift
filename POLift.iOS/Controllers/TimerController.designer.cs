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
    [Register ("TimerController")]
    partial class TimerController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Add30SecButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SkipTimerButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton Sub30SecButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel TimerStatusLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (Add30SecButton != null) {
                Add30SecButton.Dispose ();
                Add30SecButton = null;
            }

            if (SkipTimerButton != null) {
                SkipTimerButton.Dispose ();
                SkipTimerButton = null;
            }

            if (Sub30SecButton != null) {
                Sub30SecButton.Dispose ();
                Sub30SecButton = null;
            }

            if (TimerStatusLabel != null) {
                TimerStatusLabel.Dispose ();
                TimerStatusLabel = null;
            }
        }
    }
}