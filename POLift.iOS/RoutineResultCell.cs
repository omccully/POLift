using Foundation;
using System;
using UIKit;

using POLift.Core.Model;

namespace POLift.iOS
{
    public partial class RoutineResultCell : UITableViewCell
    {
        EventHandler EditClicked;

        public RoutineResultCell (IntPtr handle) : base (handle)
        {
        }

        public void Setup(IRoutineResult rr, EventHandler edit_handler)
        {
            EditClicked = edit_handler;
            EditRoutineResultButton.TouchUpInside += EditClicked;

            RoutineResultLabel.Text = rr.ToString();
        }
    }
}