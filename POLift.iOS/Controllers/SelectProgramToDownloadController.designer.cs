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
    [Register ("SelectProgramToDownloadController")]
    partial class SelectProgramToDownloadController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ProgramsTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ProgramsTableView != null) {
                ProgramsTableView.Dispose ();
                ProgramsTableView = null;
            }
        }
    }
}