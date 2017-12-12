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
    class DialogMessageService : IDialogMessageService, IDisposable
    {
        // have list of dialogs for cleanup purposes
        List<AlertDialog> Dialogs = new List<AlertDialog>();

        Activity activity;

        public DialogMessageService(Activity activity)
        {
            this.activity = activity;
        }

        public void Dispose()
        {
            if(Dialogs != null)
            {
                foreach (AlertDialog dialog in Dialogs)
                {
                    dialog.Dispose();
                }
                Dialogs.Clear();
            }

            Dialogs = null;
            activity = null;
        }

        ~DialogMessageService()
        {
            Dispose();
        }

        public void DisplayAcknowledgement(string message, Action action_when_ok = null)
        {
            if (action_when_ok != null) action_when_ok = delegate { };

            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetMessage(message);
            builder.SetNeutralButton("Ok", action_when_ok);
            //dialog.Show();

            AlertDialog dialog = builder.Create();
            dialog.Show();
            builder.Dispose();

            Dialogs.Add(dialog);
        }

        public void DisplayConfirmation(string message, Action action_if_yes, Action action_if_no = null)
        {
            //EventHandler<DialogClickEventArgs> 
            if (action_if_no == null) action_if_no = delegate { };
            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetMessage(message);
            builder.SetPositiveButton("Yes", action_if_yes);
            builder.SetNegativeButton("No", action_if_no);

            //dialog.Show();
            AlertDialog ad = builder.Create();
            ad.Show();
            builder.Dispose();

            Dialogs.Add(ad);
        }

        public void DisplayConfirmationNeverShowAgain(
            string message, string preference_key,
            Action action_if_yes,
            Action action_if_no = null)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(activity);

            bool ask = prefs.GetBoolean(AskForKey(preference_key), true);
            bool default_val = prefs.GetBoolean(DefaultKey(preference_key), false);

            if (!ask)
            {
                if (default_val)
                {
                    action_if_yes?.Invoke();
                }
                else
                {
                    action_if_no?.Invoke();
                }
                return null;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetMessage(message);

            View CheckBoxLayout = activity.LayoutInflater.Inflate(Resource.Layout.NeverShowAgainCheckBox,
                null);
            CheckBox NeverShowAgainCheckBox = CheckBoxLayout.FindViewById<CheckBox>(
                Resource.Id.NeverShowAgainCheckBox);

            builder.SetView(CheckBoxLayout);

            builder.SetPositiveButton("Yes", delegate
            {
                if (NeverShowAgainCheckBox.Checked)
                {
                    DefaultSettingTo(prefs, preference_key, true);
                }

                action_if_yes?.Invoke();
            });

            builder.SetNegativeButton("No", delegate
            {
                if (NeverShowAgainCheckBox.Checked)
                {
                    DefaultSettingTo(prefs, preference_key, false);
                }

                action_if_no?.Invoke();
            });

            AlertDialog dialog = builder.Create();
            builder.Dispose();
            dialog.Show();

            Dialogs.Add(ad);
        }

        public void DisplayConfirmationYesNotNowNever(string message, 
            string ask_for_key, Action action_if_yes)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(activity);

            bool ask = prefs.GetBoolean(ask_for_key, true);

            if (!ask)
            {
                return null;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(activity);
            builder.SetMessage(message);

            builder.SetPositiveButton("Yes", delegate
            {
                prefs.Edit().PutBoolean(ask_for_key, false).Apply();

                action_if_yes?.Invoke();
            });

            builder.SetNeutralButton("Not now", delegate
            {

            });

            builder.SetNegativeButton("Never", delegate
            {
                prefs.Edit().PutBoolean(ask_for_key, false).Apply();
            });

            AlertDialog dialog = builder.Create();
            builder.Dispose();
            dialog.Show();

            Dialogs.Add(dialog);
        }

        public void DisplayTemporaryError(string message)
        {
            DisplayTemporaryMessage(message);
        }

        public void DisplayTemporaryMessage(string message)
        {
            Toast.MakeText(activity, message, ToastLength.Long).Show();
        }

        public static string AskForKey(string key)
        {
            return $"ask_for_{key}";
        }

        public static string DefaultKey(string key)
        {
            return $"default_{key}";
        }

        static void DefaultSettingTo(ISharedPreferences prefs, string key, bool default_val)
        {
            prefs.Edit()
                .PutBoolean(AskForKey(key), false)
                .PutBoolean(DefaultKey(key), default_val)
                .Apply();
        }
    }
}