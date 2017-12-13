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

        public IDialogBuilder AddNeutralButton(string text, Action<bool> action = null)
        {
            dialog.AddButton(text);

            ButtonActions.Add(action);

            return this;
        }

        public IDialogBuilder AddPositiveButton(string text, Action<bool> action = null)
        {
            return AddNeutralButton(text, action);
        }

        public IDialogBuilder SetCheckBox(string text)
        {
            throw new NotImplementedException();
            //return this;
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
            dialog.Clicked += delegate (object sender, UIButtonEventArgs e)
            {
                ButtonActions[(int)e.ButtonIndex]?.Invoke(false);
            };

            dialog.Show();

            return this;
        }
    }
}