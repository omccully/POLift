using Foundation;
using System;
using UIKit;

using POLift.Core.Model;

namespace POLift.iOS
{
    public partial class RoutineResultCell : UITableViewCell
    {
        EventHandler EditClicked;
        EventHandler ShareClicked;

        public RoutineResultCell (IntPtr handle) : base (handle)
        {
        }

        public void Setup(IRoutineResult rr, EventHandler edit_handler, 
            EventHandler share_handler = null)
        {
            EditClicked = edit_handler;
            EditRoutineResultButton.TouchUpInside += EditClicked;

            //ShareButton.Hidden = true;

            ShareClicked = share_handler;
            if(ShareClicked != null)
            {
                ShareButton.TouchUpInside += ShareClicked;
            }
            
            RoutineResultLabel.Text = rr.ToString();

            base.SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        public override void PrepareForReuse()
        {
            EditRoutineResultButton.TouchUpInside -= EditClicked;

            if (ShareClicked != null)
            {
                ShareButton.TouchUpInside -= ShareClicked;
            }

            base.PrepareForReuse();
        }
    }
}