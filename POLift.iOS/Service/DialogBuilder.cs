using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;

namespace POLift.iOS.Service
{
    class DialogBuilder : IDialogBuilder
    {
        UIAlertView dialog;

        List<Action<bool>> ButtonActions = new List<Action<bool>>();

        public DialogBuilder()
        {
            dialog = new UIAlertView();
        }

        public IDialogBuilder AddNegativeButton(string text, Action<bool> action = null)
        {
            return AddNeutralButton(text, action);
        }

        List<string> ButtonTexts = new List<string>();
        public IDialogBuilder AddNeutralButton(string text, Action<bool> action = null)
        {
            dialog.AddButton(text);
            ButtonTexts.Add(text);
            ButtonActions.Add(action);

            return this;
        }

        public IDialogBuilder AddPositiveButton(string text, Action<bool> action = null)
        {
            return AddNeutralButton(text, action);
        }

        
        string check_box_ending = "";
        public IDialogBuilder SetCheckBox(string text)
        {
            Console.WriteLine("SetCheckBox(" + text);

            check_box_ending = text.ToLower();

            return this;
        }


        public void Dispose()
        {
           
        }

        public IDialogBuilder SetText(string text)
        {
            dialog.Message = text;
            return this;
        }

        public IDialogBuilder Show()
        {
            int options_count = ButtonActions.Count();

            if(!String.IsNullOrEmpty(check_box_ending))
            {
                foreach (string txt in ButtonTexts)
                {
                    dialog.AddButton(txt + " (" + check_box_ending + ")");
                }
            }

            dialog.Clicked += delegate (object sender, UIButtonEventArgs e)
            {
                // 0 yes
                // 1 no
                // 2 yes (never show again)
                // 3 no (never show again)
                int index = (int)e.ButtonIndex;
                if (index >= options_count)
                {
                    ButtonActions[index - options_count]?.Invoke(true);
                }
                else
                {
                    ButtonActions[index]?.Invoke(false);
                }
                
            };

            dialog.Show();

            Console.WriteLine("after Show(): dialog.Bounds.X = " + dialog.Bounds.X);
            Console.WriteLine("after Show(): dialog.Bounds.Y = " + dialog.Bounds.Y);
            return this;
        }
    }
}