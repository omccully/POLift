// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace POLift.iOS.Controllers
{
    [Register ("MainController")]
    partial class MainController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton CreateNewRoutineLink { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView RoutinesTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CreateNewRoutineLink != null) {
                CreateNewRoutineLink.Dispose ();
                CreateNewRoutineLink = null;
            }

            if (RoutinesTableView != null) {
                RoutinesTableView.Dispose ();
                RoutinesTableView = null;
            }
        }
    }
}