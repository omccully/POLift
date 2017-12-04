using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using POLift.Core.Service;

namespace POLift.iOS.Controllers
{
    interface IDatabaseUser
    {
        IPOLDatabase Database { get; set; }
    }
}