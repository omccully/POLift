using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace POLift.iOS
{
    static class AppleHelpers
    {
        public static bool DismissKeyboard(UITextField text_field)
        {
            text_field.ResignFirstResponder();
            return true;
        }
    }
}