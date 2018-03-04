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
    class DialogBuilderFactory : IDialogBuilderFactory, IDisposable
    {
        List<DialogBuilder> dialog_builders = new List<DialogBuilder>();

        Activity activity;
        public DialogBuilderFactory(Activity activity)
        {
            this.activity = activity;
        }

        public IDialogBuilder CreateDialogBuilder()
        {
            DialogBuilder db = new DialogBuilder(activity);
            dialog_builders.Add(db);
            return db;
        }

        public void Dispose()
        {
            foreach(DialogBuilder db in dialog_builders)
            {
                db.Dispose();
            }

            dialog_builders = null;
        }
    }
}