using Foundation;
using System;
using UIKit;

namespace POLift.iOS
{
    public partial class NavigationCell : UITableViewCell
    {
        public NavigationCell (IntPtr handle) : base (handle)
        {
        }

        public void Setup(UIImage image, string text)
        {
            NavigationIconImageView.Image = image;
            NavigationTextLabel.Text = text;
            NavigationTextLabel.TextColor = UIColor.White;

            base.BackgroundColor = UIColor.Clear;

            base.SelectionStyle = UITableViewCellSelectionStyle.None;
        }
    }
}