using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using POLift.Core.Service;

namespace POLift.Droid.Service
{
    class DialogBuilder : IDialogBuilder
    {
        Activity activity;
        AlertDialog.Builder builder;

        public DialogBuilder(Activity activity)
        {
            this.activity = activity;
            builder = new AlertDialog.Builder(activity);
        }

        public IDialogBuilder SetText(string text)
        {
            builder.SetMessage(text);
            return this;
        }

        public IDialogBuilder AddPositiveButton(string text, Action<bool> action = null)
        {
            
            builder.SetPositiveButton(text, delegate { action?.Invoke(CheckBoxChecked); });
            return this;
        }

        public IDialogBuilder AddNeutralButton(string text, Action<bool> action = null)
        {
            builder.SetNeutralButton(text, delegate { action?.Invoke(CheckBoxChecked); });
            return this;
        }

        public IDialogBuilder AddNegativeButton(string text, Action<bool> action = null)
        {
            builder.SetNegativeButton(text, delegate { action?.Invoke(CheckBoxChecked); });
            return this;
        }

        bool CheckBoxChecked
        {
            get
            {
                if (NeverShowAgainCheckBox == null) return false;

                return NeverShowAgainCheckBox.Checked;
            }
        }
        
        CheckBox NeverShowAgainCheckBox = null;
        public IDialogBuilder SetCheckBox(string text)
        {
           
            View CheckBoxLayout = activity.LayoutInflater.Inflate(Resource.Layout.NeverShowAgainCheckBox,
                null);
            NeverShowAgainCheckBox = CheckBoxLayout.FindViewById<CheckBox>(
                Resource.Id.NeverShowAgainCheckBox);
            NeverShowAgainCheckBox.Text = text;

            builder.SetView(CheckBoxLayout);

            return this;
        }

        AlertDialog dialog = null;
        public IDialogBuilder Show()
        {
            dialog = builder.Create();
            dialog.Show();
            builder.Dispose();

            return this;
        }

        public void Dispose()
        {
            try
            {
                dialog?.Dismiss();
                dialog?.Dispose();
            }
            catch { }
           
            builder?.Dispose();
            builder = null;
            NeverShowAgainCheckBox = null;
        }
    }
}