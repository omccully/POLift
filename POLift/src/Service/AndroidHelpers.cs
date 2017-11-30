﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Preferences;

namespace POLift.Service
{
    using Core.Service;
    using Core.Model;

    public static class AndroidHelpers
    {
        public static AlertDialog DisplayError(Context context, string message,
            EventHandler<DialogClickEventArgs> action_when_ok = null)
        {
            if (action_when_ok != null) action_when_ok = delegate { };

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetNeutralButton("Ok", action_when_ok);
            //dialog.Show();

            AlertDialog dialog = builder.Create();
            dialog.Show();
            builder.Dispose();
            return dialog;
        }

        public static AlertDialog DisplayConfirmation(Context context, string message,
            EventHandler<DialogClickEventArgs> action_if_yes,
            EventHandler<DialogClickEventArgs> action_if_no = null)
        {
            if (action_if_no == null) action_if_no = delegate { };
            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetMessage(message);
            builder.SetPositiveButton("Yes", action_if_yes);
            builder.SetNegativeButton("No", action_if_no);

            //dialog.Show();
            AlertDialog ad = builder.Create();
            ad.Show();
            builder.Dispose();
            return ad;
        }

        public static AlertDialog DisplayConfirmationNeverShowAgain(Activity activity, 
            string message, string preference_key, 
            Action action_if_yes,
            Action action_if_no = null)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(activity);

            bool ask = prefs.GetBoolean(AskForKey(preference_key), true);
            bool default_val = prefs.GetBoolean(DefaultKey(preference_key), false);

            if(!ask)
            {
                if(default_val)
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
            return dialog;
        }

        public static AlertDialog DisplayConfirmationYesNotNowNever(Context context,
            string message, string ask_for_key,
            Action action_if_yes)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            bool ask = prefs.GetBoolean(ask_for_key, true);

            if (!ask)
            {
                return null;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
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
            return dialog;
        }

        static string AskForKey(string key)
        {
            return $"ask_for_{key}";
        }

        static string DefaultKey(string key)
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

        public delegate void PreferencesSetter(ISharedPreferencesEditor editor);
        public static void Set(this ISharedPreferences prefs, PreferencesSetter setter)
        {
            ISharedPreferencesEditor editor = prefs.Edit();
            setter(editor);
            editor.Apply();
        }


        public static void ImportFromUri(Android.Net.Uri uri, IPOLDatabase Database, ContentResolver content_resolver,
            string temp_dir, IFileOperations fops, bool full = true)
        {
            const string ImportFile = "database-import.db3";
            string ImportFilePath = Path.Combine(temp_dir, ImportFile);

            fops.Write(ImportFilePath, content_resolver.OpenInputStream(uri));

            Helpers.ImportDatabaseFromLocalFile(ImportFilePath, Database, full);

            try
            {
                fops.Delete(ImportFilePath);
            }
            catch { }
        }
    }
}