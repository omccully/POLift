using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Foundation;
using UIKit;

using GalaSoft.MvvmLight.Helpers;
using System.Drawing;
using POLift.Core.Model;
using Social;
using POLift.iOS.Service;
using POLift.Core.Service;

using POLift.Core.ViewModel;

namespace POLift.iOS
{
    static class AppleHelpers
    {
        public static void OpenHelpAndFeedback()
        {
            UIApplication.SharedApplication.OpenUrl(
                new NSUrl(SideMenuViewModel.HelpUrl));
        }

        public static void ShareRoutineResult(this UIViewController vc,
            IRoutineResult rr)
        {
            if (SLComposeViewController.IsAvailable(SLServiceKind.Twitter))
            {
                Console.WriteLine("Twitter available");
                vc.ShareRoutineResult(rr, SLServiceKind.Twitter);
            }
            else if (SLComposeViewController.IsAvailable(SLServiceKind.Facebook))
            {
                Console.WriteLine("Facebook available");
                vc.ShareRoutineResult(rr, SLServiceKind.Twitter);
            }
            else
            {
                Console.WriteLine("No social media services available");
                Toaster toaster = new Toaster();
                toaster.DisplayError("No Facebook/Twitter services available");
            }
        }

        public static void ShareRoutineResult(this UIViewController vc,
            IRoutineResult rr, SLServiceKind kind)
        {
            SLComposeViewController slcvc = SLComposeViewController.FromService(kind);
            slcvc.CompletionHandler = delegate
            {
                vc.DismissViewController(true, null);
                Console.WriteLine("Cancelling");
            };

            slcvc.SetInitialText(rr.ShareText());
            // add url once website gets set up slcvc.AddUrl()

            vc.PresentViewController(slcvc, true, null);
        }

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


        static UIBarButtonItem DoneBarButton(UITextField text_field, UIBarButtonSystemItem but = UIBarButtonSystemItem.Done)
        {
            return new UIBarButtonItem(but, delegate
            {
                text_field.ResignFirstResponder();
            });
        }

        public static void AddDoneButtonToNumericKeyboard(this UITextField text_field)
        {
            AddButtonToKeyboard(text_field, DoneBarButton(text_field));
        }

        public static void AddButtonToKeyboard(this UITextField text_field, UIBarButtonItem bar_but_item)
        {
            AddButtonsToKeyboard(text_field, new UIBarButtonItem[] {
                new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
                bar_but_item
            });
        }

        public static void AddButtonsToKeyboard(this UITextField text_field, UIBarButtonItem[] bar_button_items)
        {
            UIToolbar toolbar = new UIToolbar(new RectangleF(0.0f, 0.0f, 50.0f, 44.0f));

            toolbar.Items = bar_button_items;

            text_field.InputAccessoryView = toolbar;
        }

        public static void AddSubmitCancelButtonsToNumericKeyboard(this UITextField text_field, EventHandler enter_action)
        {
            AddButtonsToKeyboard(text_field, new UIBarButtonItem[]
            {
                DoneBarButton(text_field, UIBarButtonSystemItem.Cancel),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Submit", UIBarButtonItemStyle.Done, enter_action)
            });
        }
    }
}