using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;

namespace POLift.iOS.Service
{
    class ControllerDialogBuilder : IDialogBuilder
    {
        UIViewController parent;
        UIAlertController alert_controller;

        public ControllerDialogBuilder(UIViewController parent)
        {
            this.parent = parent;
            alert_controller = UIAlertController.Create("",
              current_text, UIAlertControllerStyle.Alert);
        }

        bool CheckBoxChecked
        {
            get
            {
                if(cb_switch == null) return false;
                return cb_switch.On;
            }
        }

        public IDialogBuilder AddNegativeButton(string text, Action<bool> action = null)
        {
            alert_controller.AddAction(UIAlertAction.Create(text, UIAlertActionStyle.Destructive, delegate
            {
                action?.Invoke(CheckBoxChecked);
            }));
            return this;
        }

        public IDialogBuilder AddNeutralButton(string text, Action<bool> action = null)
        {
            alert_controller.AddAction(UIAlertAction.Create(text, UIAlertActionStyle.Cancel, delegate
            {
                action?.Invoke(CheckBoxChecked);
            }));
            return this;
        }

        public IDialogBuilder AddPositiveButton(string text, Action<bool> action = null)
        {
            alert_controller.AddAction(UIAlertAction.Create(text, UIAlertActionStyle.Default, delegate
            {
                action?.Invoke(CheckBoxChecked);
            }));
            return this;
        }

        public void Dispose()
        {
            
        }

        UISwitch cb_switch = null;
        public IDialogBuilder SetCheckBox(string text)
        {
            UIView dest = alert_controller.View
                .Subviews[0].Subviews[0].Subviews[0].Subviews[0].Subviews[0];
            Console.WriteLine("dest.Frame = " + dest.Frame);
            Console.WriteLine("dest.Frame.Height = " + dest.Frame.Height);
            cb_switch = new UISwitch(/*new CoreGraphics.CGRect(0, 0, 0, 0)*/ /*new CoreGraphics.CGRect(0, 0, 50, 25)*/);
            cb_switch.On = false;

            UILabel switch_label = new UILabel(new CoreGraphics.CGRect(0, 0, 180, 25) /*new CoreGraphics.CGRect(55, 0, 180, 25)*/);
            
            switch_label.Text = text;

            //dest.BackgroundColor = UIColor.Brown;

            //dest.AddSubview(cb_switch);
            //dest.AddSubview(switch_label);
            //UILayoutGuide guide = dest.LayoutMarginsGuide;
            //dest.Frame = new CoreGraphics.CGRect(0, 0, 350, 350);

            //dest.HeightAnchor.ConstraintGreaterThanOrEqualTo(450);

            foreach (UIView view in dest.Subviews)
            {
                UILabel label = view as UILabel;
                if (label != null)
                {
                    Console.WriteLine(label.Text);

                    if (label.Text == current_text)
                    {
                        Console.WriteLine("===========");
                        //sw.AddConstraint(NSLayoutConstraint.Create(label))

                        //label.BottomAnchor.ConstraintEqualTo(switch_label.TopAnchor).Active = true;
                        //switch_label.TopAnchor.ConstraintEqualTo(label.BottomAnchor).Active = true;

                        // dest.TopAnchor.ConstraintEqualTo(label.TopAnchor).Active = true;
                        //dest.BottomAnchor.ConstraintEqualTo(switch_label.BottomAnchor).Active = true;

                        switch_label.TopAnchor.ConstraintEqualTo(label.BottomAnchor, -10).Active = true;

                        dest.BottomAnchor.ConstraintEqualTo(switch_label.BottomAnchor).Active = true;
                        dest.TopAnchor.ConstraintEqualTo(label.TopAnchor).Active = true;
                        //dest.LeftAnchor.ConstraintEqualTo(label.LeftAnchor).Active = true;
                        //dest.Right

                        /*NSLayoutConstraint temp;

                        label.LeftAnchor.ConstraintEqualTo(dest.LeftAnchor).Active = true;
                        label.RightAnchor.ConstraintEqualTo(dest.RightAnchor).Active = true;
                        label.TopAnchor.ConstraintEqualTo(dest.TopAnchor).Active = true;
                        temp = label.BottomAnchor.ConstraintEqualTo(dest.BottomAnchor);
                        temp.Constant = -50;

                        temp.Active = true;*/

                        //cb_switch.TopAnchor.ConstraintEqualTo(label.BottomAnchor).Active = true;
                        //cb_switch.BottomAnchor.ConstraintEqualTo(dest.BottomAnchor).Active = true;
                        //cb_switch.LeftAnchor.ConstraintEqualTo(dest.LeftAnchor).Active = true;
                        // cb_switch.RightAnchor.ConstraintEqualTo(dest.RightAnchor).Active = true;

                        //switch_label.TopAnchor.ConstraintEqualTo(label.BottomAnchor).Active = true;


                        /*NSLayoutConstraint temp = cb_switch.TopAnchor.ConstraintEqualTo(label.BottomAnchor);
                        Console.WriteLine("prev Constant = " + temp.Constant);
                        Console.WriteLine("prev Multiplier = " + temp.Multiplier);
                        temp.Constant = new nfloat(0.0);
                        temp.Active = true;

                        cb_switch.LeftAnchor.ConstraintEqualTo(dest.LeftAnchor).Active = true;

                        temp = switch_label.TopAnchor.ConstraintEqualTo(label.BottomAnchor);
                        Console.WriteLine("prev Constant = " + temp.Constant);
                        Console.WriteLine("prev Multiplier = " + temp.Multiplier);
                        temp.Constant = new nfloat(0.0);
                        temp.Active = true;


                        temp = switch_label.HeightAnchor.ConstraintEqualTo(label.HeightAnchor);
                        Console.WriteLine("prev Constant = " + temp.Constant);
                        Console.WriteLine("prev Multiplier = " + temp.Multiplier);
                        temp.Constant = new nfloat(0.0);
                        temp.Active = true;*/

                        // dest.BottomAnchor.ConstraintEqualTo(switch_label.BottomAnchor);
                    }
                }
            }

            return this;
        }

        string current_text = "Message\n...\n...";
        public IDialogBuilder SetText(string text)
        {
            alert_controller.Message = current_text = text + "\n...\n...";
            return this;
        }

        public IDialogBuilder Show()
        {
            parent.PresentViewController(alert_controller, true, null);
            AppleHelpers.PrintViewHierarchy(alert_controller.View);
            return this;
        }
    }
}