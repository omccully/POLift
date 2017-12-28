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
    [Register ("ViewRoutineResultsController")]
    partial class ViewRoutineResultsController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView RoutineResultsTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (RoutineResultsTableView != null) {
                RoutineResultsTableView.Dispose ();
                RoutineResultsTableView = null;
            }
        }
    }
}