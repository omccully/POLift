using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using System.Drawing;

namespace POLift.iOS.Controllers
{
    public class NumericKeyboardViewController : UIViewController
    {
        public NumericKeyboardViewController(IntPtr handle) : base(handle)
        {

        }

        protected void AddDoneButtonToNumericKeyboard(UITextField text_field)
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
    }
}