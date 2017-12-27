using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;

namespace POLift.iOS.Service
{
    public class MainThreadInvoker : IMainThreadInvoker
    {
        NSObject ui_obj;
        public MainThreadInvoker(NSObject ui_obj)
        {
            this.ui_obj = ui_obj;
        }

        public void Invoke(Action action)
        {
            ui_obj.BeginInvokeOnMainThread(action);
        }
    }
}