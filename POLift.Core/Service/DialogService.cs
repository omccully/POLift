using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public class DialogService : IDialogService
    {
        public IDialogBuilderFactory Factory { get; private set; }
        public KeyValueStorage KeyValueStorage { get; private set; }

        List<IDialogBuilder> builders = new List<IDialogBuilder>();

        public DialogService(IDialogBuilderFactory Factory, KeyValueStorage KeyValueStorage)
        {
            this.Factory = Factory;
            this.KeyValueStorage = KeyValueStorage;
        }

        public void DisplayAcknowledgement(string message, Action action_when_ok = null, string okay_button = "OK")
        {
            builders.Add(Factory.CreateDialogBuilder()
                .SetText(message)
                .AddNeutralButton(okay_button, Helpers.ToBoolAction(action_when_ok))
                .Show());
        }

        public void DisplayConfirmation(string message, Action action_if_yes, Action action_if_no = null)
        {
            builders.Add(Factory.CreateDialogBuilder()
                .SetText(message)
                .AddPositiveButton("Yes", Helpers.ToBoolAction(action_if_yes))
                .AddNegativeButton("No", Helpers.ToBoolAction(action_if_no))
                .Show());
        }

        public void DisplayConfirmationNeverShowAgain(string message, string preference_key, Action action_if_yes, Action action_if_no = null)
        {
            bool ask, default_val;
            if (KeyValueStorage == null)
            {
                ask = true;
                default_val = false;
            }
            else
            {
                ask = KeyValueStorage.GetBoolean(AskForKey(preference_key), true);
                default_val = KeyValueStorage.GetBoolean(DefaultKey(preference_key), false);
            }
            
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
                return;
            }

            IDialogBuilder builder = Factory.CreateDialogBuilder()
                .SetText(message);

            builder.SetCheckBox("Never show again");

            builder.AddPositiveButton("Yes", delegate (bool never_show_again)
            {
                if(never_show_again)
                {
                    DefaultSettingTo(preference_key, true);
                }
                action_if_yes?.Invoke();
            });

            builder.AddNegativeButton("No", delegate (bool never_show_again)
            {
                if (never_show_again)
                {
                    DefaultSettingTo(preference_key, false);
                }
                action_if_no?.Invoke();
            });

            builders.Add(builder);
            builder.Show();
        }

        public void DisplayConfirmationYesNotNowNever(string message, string ask_for_key, Action action_if_yes)
        {
            bool ask = KeyValueStorage == null ? true : 
                KeyValueStorage.GetBoolean(ask_for_key, true);

            if (!ask) return;

            IDialogBuilder builder = Factory.CreateDialogBuilder()
                .SetText(message);

            builder.AddPositiveButton("Yes", delegate
            {
                // never ask again
                KeyValueStorage?.SetValue(ask_for_key, false);

                action_if_yes?.Invoke();
            });

            builder.AddNeutralButton("Not now", delegate
            {

            });

            builder.AddNegativeButton("Never", delegate
            {
                KeyValueStorage?.SetValue(ask_for_key, false);
            });

            builders.Add(builder);
            builder.Show();
        }

        public void DisplayConfirmationYesNoYesNeverShowAgain(string message, string ask_for_key, 
            Action action_if_yes, Action action_if_no = null)
        {
            bool ask = KeyValueStorage == null ? true :
                KeyValueStorage.GetBoolean(ask_for_key, true);

            if (!ask)
            {
                // if don't ask, assume yes
                action_if_yes?.Invoke();
                return;
            }

            IDialogBuilder builder = Factory.CreateDialogBuilder()
                .SetText(message);

            builder.AddNeutralButton("Yes", delegate
            {
                action_if_yes?.Invoke();
            });

            builder.AddPositiveButton("No", delegate
            {
                action_if_no?.Invoke();
            });

            builder.AddNegativeButton("Yes, never show again", delegate
            {
                // never ask again
                KeyValueStorage?.SetValue(ask_for_key, false);

                action_if_yes?.Invoke();
            });

            builders.Add(builder);
            builder.Show();
        }

        public void DisplayAcknowledgementOnce(string message, string key)
        {
            bool acknowledged = KeyValueStorage.GetBoolean(key, false);

            if (!acknowledged)
            {
                DisplayAcknowledgement(message,
                    delegate
                    {
                        KeyValueStorage.SetValue(key, true);
                    },
                    "OK (never show again)");
            }
        }

        public void Dispose()
        {
            if (builders == null) return; // already disposed

            foreach(IDialogBuilder builder in builders)
            {
                builder.Dispose();
            }

            builders = null;
            Factory = null;
        }

        public static string AskForKey(string key)
        {
            return $"ask_for_{key}";
        }

        public static string DefaultKey(string key)
        {
            return $"default_{key}";
        }

        void DefaultSettingTo(string key, bool default_val)
        {
            if(KeyValueStorage != null)
            {
                KeyValueStorage.SetValue(AskForKey(key), false);
                KeyValueStorage.SetValue(DefaultKey(key), default_val);
            }
        }
    }
}
