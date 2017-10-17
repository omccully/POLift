using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Model
{
    class Navigation : INavigation
    {
        public string Text { get; set; }
        public int IconResourceID { get; set; }

        public event EventHandler Click;

        public Navigation(string Text, EventHandler event_handler, int icon_resource_id=0)
        {
            this.Text = Text;
            Click += event_handler;
            this.IconResourceID = icon_resource_id;
        }

        public void OnClick(EventArgs e = null)
        {
            if (e == null) e = new EventArgs();
            Click?.Invoke(this, e);
        }
    }
}