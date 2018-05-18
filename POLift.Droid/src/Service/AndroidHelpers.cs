using System;
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
using Android.Support.V4.Content;
using Android.Support.V4.App;

namespace POLift.Droid.Service
{
    using Core.Service;
    using Core.Model;

    public static class AndroidHelpers
    {
        //public static Intent 

        public static void ShareRoutineResult(this Context context, IRoutineResult rr)
        {
            Intent tweet = new Intent(Intent.ActionSend);
            tweet.PutExtra(Intent.ExtraText, rr.ShareText());
            tweet.SetType("text/plain");
            context.StartActivity(Intent.CreateChooser(tweet, "Share this via"));
        }

        public static void BackupData(Activity activity)
        {
            try
            {
                Java.IO.File export_file = new Java.IO.File(C.DatabasePath);
                Android.Net.Uri uri = FileProvider.GetUriForFile(activity,
                    "com.cml.poliftprovider", export_file);

                Intent share_intent = ShareCompat.IntentBuilder.From(activity)
                    .SetType("application/octet-stream")
                    .SetStream(uri)
                    .Intent;

                share_intent.SetData(uri);
                share_intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                activity.StartActivity(share_intent);
            }
            catch (ActivityNotFoundException err)
            {
                DisplayError(activity, err.Message);
            }
        }

        public static void NavigateToAppRating(Context context)
        {
            try
            {
                ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

                context.StartActivity(new Intent(Intent.ActionView,
                    Android.Net.Uri.Parse("market://details?id=com.cml.polift")));

                prefs.Edit().PutBoolean("has_rated_app", true).Apply();
            }
            catch(Exception e)
            {
                Toast.MakeText(context, "Error: " + e.Message,
                    ToastLength.Long);
            }
        }

        public static string Inspect(this Bundle bundle)
        {
            if (bundle == null) return "null";

            StringBuilder sb = new StringBuilder();

            foreach (string key in bundle.KeySet())
            {
                Java.Lang.Object obj = bundle.Get(key);
                if(obj == null)
                {
                    sb.AppendLine($"{key} = null");
                }
                else
                {
                    sb.AppendLine($"{key} = {obj.ToString()}  ({obj.GetType()})");
                }
            }

            return sb.ToString();
        }

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

        public static void SetActivityDepth(Context context, int val)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            prefs.Edit().PutInt("perform_routine_activity_depth", val).Apply();
        }

        public static int GetActivityDepth(Context context)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            return prefs.GetInt("perform_routine_activity_depth", 0);
        }
    }
}