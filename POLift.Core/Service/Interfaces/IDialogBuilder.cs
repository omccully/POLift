using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public interface IDialogBuilder : IDisposable
    {
        IDialogBuilder SetText(string text);

        IDialogBuilder AddPositiveButton(string text, Action<bool> action = null);

        IDialogBuilder AddNeutralButton(string text, Action<bool> action = null);

        IDialogBuilder AddNegativeButton(string text, Action<bool> action = null);

        IDialogBuilder SetCheckBox(string text);

        IDialogBuilder Show();
    }
}
