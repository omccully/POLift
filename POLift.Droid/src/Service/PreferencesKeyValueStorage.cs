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
    class PreferencesKeyValueStorage : KeyValueStorage
    {
        ISharedPreferences Prefs;

        public PreferencesKeyValueStorage(ISharedPreferences prefs)
        {
            this.Prefs = prefs;
        }

        public override KeyValueStorage SetValue(string key, int val)
        {
            Prefs.Edit()
                .PutInt(key, val)
                .Apply();
            return this;
        }

        public override KeyValueStorage SetValue(string key, string val)
        {
            Prefs.Edit()
                .PutString(key, val)
                .Apply();
            return this;
        }

        public override int GetInteger(string key, int default_val = 0)
        {
            return Prefs.GetInt(key, default_val);
        }

        public override string GetString(string key, string default_val = null)
        {
            return Prefs.GetString(key, default_val);
        }

        public override bool GetBoolean(string key, bool default_val = false)
        {
            return Prefs.GetBoolean(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, bool val)
        {
            Prefs.Edit()
                .PutBoolean(key, val)
                .Apply();
            return this;
        }

    }
}