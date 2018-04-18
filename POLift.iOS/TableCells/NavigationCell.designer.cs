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
    [Register ("NavigationCell")]
    partial class NavigationCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView NavigationIconImageView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NavigationTextLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (NavigationIconImageView != null) {
                NavigationIconImageView.Dispose ();
                NavigationIconImageView = null;
            }

            if (NavigationTextLabel != null) {
                NavigationTextLabel.Dispose ();
                NavigationTextLabel = null;
            }
        }
    }
}