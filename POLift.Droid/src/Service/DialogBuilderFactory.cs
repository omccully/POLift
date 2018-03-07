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
    class DialogBuilderFactory : IDialogBuilderFactory
    {
        Activity activity;

        public DialogBuilderFactory(Activity activity)
        {
            this.activity = activity;
        }

        public IDialogBuilder CreateDialogBuilder()
        {
            return new DialogBuilder(activity);
        }
    }
}