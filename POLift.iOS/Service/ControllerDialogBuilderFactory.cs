using System;
using System.Collections.Generic;
using System.Text;

using POLift.Core.Service;
using UIKit;

namespace POLift.iOS.Service
{
    class ControllerDialogBuilderFactory : IDialogBuilderFactory
    {
        UIViewController parent;
        public ControllerDialogBuilderFactory(UIViewController parent)
        {
            this.parent = parent;
        }

        public IDialogBuilder CreateDialogBuilder()
        {
            return new ControllerDialogBuilder(parent);
        }
    }
}
