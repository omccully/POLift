using Foundation;
using System;
using UIKit;
using POLift.Core.Model;
using System.Text.RegularExpressions;

namespace POLift.iOS
{
    public partial class RoutineCell : UITableViewCell
    {
        EventHandler EditClicked;

        public RoutineCell (IntPtr handle) : base (handle)
        {
        }

        public void Setup(IRoutineWithLatestResult routineinfo, EventHandler edit_handler)
        {
            EditClicked = edit_handler;
            EditRoutinebutton.TouchUpInside += EditClicked;

            //

            Regex regex = new Regex(@"\[.*\]");
            string routine_txt = routineinfo.Routine.ToString();
            RoutineLabel.Text = regex.Replace(routine_txt, "").Trim();

            IRoutineResult latest = routineinfo.LatestResult;

            if (latest == null)
            {
                LastPerformTimeLabel.Text = "Never performed";
            }
            else
            {
                LastPerformTimeLabel.Text = latest.RelativeTimeDetails;
            }

            if (LastPerformTimeLabel.Text.Contains("Uncompleted") &&
                !LastPerformTimeLabel.Text.Contains("day"))
            {
                LastPerformTimeLabel.TextColor = UIColor.Red;
            }
            else
            {
                LastPerformTimeLabel.TextColor = UIColor.Black;
            }
        }

        public override void PrepareForReuse()
        {
            if (EditClicked != null)
            {
                EditRoutinebutton.TouchUpInside -= EditClicked;
            }

            EditClicked = null;

            base.PrepareForReuse();
        }

    }
}