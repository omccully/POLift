using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Foundation;
using UIKit;

using GalaSoft.MvvmLight.Helpers;

namespace POLift.iOS
{
    static class AppleHelpers
    {
        public static bool DismissKeyboard(UITextField text_field)
        {
            text_field.ResignFirstResponder();
            return true;
        }

        public static Binding TwoWayBinding(this UITextField text_field,
            UIViewController cont, Expression<Func<string>> view_model_field)
        {
            text_field.EditingChanged += (s, e) => { };

            return cont.SetBinding<string,string>(
                () => text_field.Text,
                view_model_field,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged");
        }
    }
}