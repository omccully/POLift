using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Foundation;
using UIKit;

using GalaSoft.MvvmLight.Helpers;
using System.Drawing;

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

        public static void AddDoneButtonToNumericKeyboard(this UITextField text_field)
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));
            var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, delegate
            {
                text_field.ResignFirstResponder();
            });

            toolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                doneButton
            };

            text_field.InputAccessoryView = toolbar;
        }

        /*public static Binding TwoWayBinding(this UIViewController cont,
             Expression<Func<string>> text_field, Expression<Func<string>> view_model_prop)
        {
            //text_field..EditingChanged += (s, e) => { };

            return cont.SetBinding<string, string>(
                () => text_field.Text,
                view_model_field,
                BindingMode.TwoWay)
                .ObserveSourceEvent("EditingChanged");
        }*/
    }
}