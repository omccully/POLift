using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public interface IDialogService : IDisposable
    {
        KeyValueStorage KeyValueStorage { get; }

        IDialogBuilderFactory Factory { get; }

        void DisplayAcknowledgement(string message, Action action_when_ok = null, string okay_button = "OK");

        void DisplayConfirmation(string message, Action action_if_yes, Action action_if_no = null);

        void DisplayConfirmationNeverShowAgain(string message, string preference_key, Action action_if_yes, Action action_if_no = null);

        void DisplayConfirmationYesNotNowNever(string message, string ask_for_key, Action action_if_yes);

        void DisplayAcknowledgementYesNotNowNeverByTimeSpan(string message, string key, TimeSpan span, Action action_if_ok);

        void DisplayConfirmationYesNoYesNeverShowAgain(string message, string ask_for_key,
            Action action_if_yes, Action action_if_no = null);
    }
}
