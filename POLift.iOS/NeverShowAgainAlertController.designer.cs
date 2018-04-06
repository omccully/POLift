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
    [Register ("NeverShowAgainAlertController")]
    partial class NeverShowAgainAlertController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch CheckBoxSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel MessageLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton NoButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel SwitchLabelmk { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton YesButton { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CheckBoxSwitch != null) {
                CheckBoxSwitch.Dispose ();
                CheckBoxSwitch = null;
            }

            if (MessageLabel != null) {
                MessageLabel.Dispose ();
                MessageLabel = null;
            }

            if (NoButton != null) {
                NoButton.Dispose ();
                NoButton = null;
            }

            if (SwitchLabelmk != null) {
                SwitchLabelmk.Dispose ();
                SwitchLabelmk = null;
            }

            if (YesButton != null) {
                YesButton.Dispose ();
                YesButton = null;
            }
        }
    }
}