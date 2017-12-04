using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace POLift.iOS
{
    interface IValueReturner<T>
    {
        event Action<T> ValueChosen;
    }
}