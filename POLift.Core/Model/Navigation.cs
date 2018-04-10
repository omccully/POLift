using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public class Navigation : INavigation
    {
        public string Text { get; set; }
        public int IconResourceID { get; set; }

        public EventHandler Click { get; set; }

        public bool IsDivider => Text == null;

        public Navigation() // divider
        {
            this.Text = null;
            Click = null;
            this.IconResourceID = 0;
        }

        public Navigation(string Text, EventHandler event_handler, int icon_resource_id=0)
        {
            this.Text = Text;
            Click = event_handler;
            this.IconResourceID = icon_resource_id;
        }

        public Navigation(string Text, Action event_handler, int icon_resource_id = 0)
        {
            this.Text = Text;
            Click = delegate { event_handler(); };
            this.IconResourceID = icon_resource_id;
        }

        public void OnClick(EventArgs e = null)
        {
            if (e == null) e = new EventArgs();
            Click?.Invoke(this, e);
        }
    }
}