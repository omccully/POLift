﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public interface IDialogService : IDisposable
    {
        //IDialogBuilderFactory Factory { get; }

        void DisplayAcknowledgement(string message, Action action_when_ok = null);

        void DisplayConfirmation(string message, Action action_if_yes, Action action_if_no = null);

        void DisplayConfirmationNeverShowAgain(string message, string preference_key, Action action_if_yes, Action action_if_no = null);

        void DisplayConfirmationYesNotNowNever(string message, string ask_for_key, Action action_if_yes);
    }
}
