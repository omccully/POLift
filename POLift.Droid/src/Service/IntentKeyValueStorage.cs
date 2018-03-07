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
    public class IntentKeyValueStorage : KeyValueStorage
    {
        Intent intent;

        public IntentKeyValueStorage(Intent intent)
        {
            if (intent == null) throw new ArgumentNullException("Intent is null");
            this.intent = intent;
        }

        public override KeyValueStorage SetValue(string key, string val)
        {
            intent.PutExtra(key, val);
            return this;
        }

        public override string GetString(string key, string default_val = null)
        {
            string result = intent.GetStringExtra(key);

            return (result == null ? default_val : result);
        }

        public override KeyValueStorage SetValue(string key, int val)
        {
            intent.PutExtra(key, val);
            return this;
        }

        public override int GetInteger(string key, int default_val = 0)
        {
            return intent.GetIntExtra(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, bool val)
        {
            intent.PutExtra(key, val);
            return this;
        }

        public override bool GetBoolean(string key, bool default_val = false)
        {
            return intent.GetBooleanExtra(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, int[] val)
        {
            intent.PutExtra(key, val);
            return this;
        }

        public override int[] GetIntArray(string key, int[] default_val = null)
        {
            int[] result = intent.GetIntArrayExtra(key);
            if (result == null) return default_val;
            return result;
        }

        public override KeyValueStorage SetValue(string key, float val)
        {
            intent.PutExtra(key, val);
            return this;
        }

        public override float GetFloat(string key, float default_val = 0)
        {
            return intent.GetFloatExtra(key, default_val);
        }
    }
}