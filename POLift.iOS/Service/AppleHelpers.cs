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
        public static UIView CustomSnapshotFromView(this UIView view)
        {
            UIGraphics.BeginImageContextWithOptions(view.Bounds.Size, false, new nfloat(0));
            view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            UIImage img = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            UIView snapshot = new UIImageView(img);
            snapshot.Layer.MasksToBounds = false;
            snapshot.Layer.CornerRadius = new nfloat(0.0);
            snapshot.Layer.ShadowOffset = new CoreGraphics.CGSize(-5.0, 0.0);
            snapshot.Layer.ShadowRadius = new nfloat(5.0);
            snapshot.Layer.Opacity = 0.4f;

            return snapshot;
        }

        public static void PrintViewHierarchy(UIView view, int depth = 0)
        {
            foreach (UIView subview in view.Subviews)
            {
                for (int i = 0; i < depth; i++) Console.Write('\t');
                Console.Write(subview.Class.Name + " " + subview.Bounds + " " + subview.Frame);
                foreach(NSLayoutConstraint constraint in subview.Constraints)
                {
                    Console.Write(" " + constraint.DebugDescription);
                }
                Console.WriteLine();
                PrintViewHierarchy(subview, depth + 1);
            }
        }

        public static void OpenRateApp()
        {
            NSUrl url = new NSUrl("itms-apps://itunes.apple.com/app/id1352415628");
            UIApplication.SharedApplication.OpenUrl(url);
        }

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