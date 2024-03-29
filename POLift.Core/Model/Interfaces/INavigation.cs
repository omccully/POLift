﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public interface INavigation
    {
        string Text { get; set; }
        int IconResourceID { get; set; }
        string IconIdentifier { get; set; }
        EventHandler Click { get; set; }
        void OnClick(EventArgs e = null);

        bool IsDivider { get; }
    }
}