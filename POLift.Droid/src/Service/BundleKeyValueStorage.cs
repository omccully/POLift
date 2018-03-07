using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using POLift.Core.Service;

namespace POLift.Droid.Service
{
    class BundleKeyValueStorage : KeyValueStorage
    {
        public readonly Bundle Bundle;

        public BundleKeyValueStorage(Bundle bundle)
        {
            if (bundle == null) throw new ArgumentNullException("Bundle is null");
            this.Bundle = bundle;
        }

        public BundleKeyValueStorage(Intent intent)
        {
            if (intent == null) throw new ArgumentNullException("Intent is null");
            if (intent.Extras == null)
            {
                intent.PutExtra("ignored_extra", true);

                //intent.PutExtras(new Bundle());
                if (intent.Extras == null) throw new Exception("didn't work lol");
                this.Bundle = intent.Extras;
            }
        }

        public override KeyValueStorage SetValue(string key, string val)
        {
            Bundle.PutString(key, val);
            return this;
        }

        public override string GetString(string key, string default_val = null)
        {
            return Bundle.GetString(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, int val)
        {
            Bundle.PutInt(key, val);
            return this;
        }

        public override int GetInteger(string key, int default_val = 0)
        {
            return Bundle.GetInt(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, bool val)
        {
            Bundle.PutBoolean(key, val);
            return this;
        }

        public override bool GetBoolean(string key, bool default_val = false)
        {
            return Bundle.GetBoolean(key, default_val);
        }

        public override KeyValueStorage SetValue(string key, int[] val)
        {
            Bundle.PutIntArray(key, val);
            return this;
        }

        public override int[] GetIntArray(string key, int[] default_val = null)
        {
            int[] result = Bundle.GetIntArray(key);
            if (result == null) return default_val;
            return result;
        }

        public override KeyValueStorage SetValue(string key, float val)
        {
            Bundle.PutFloat(key, val);
            return this;
        }

        public override float GetFloat(string key, float default_val = 0)
        {
            return Bundle.GetFloat(key, default_val);
        }

        public static ChainedKeyValueStorage ChainedFromStates(Bundle savedInstanceState,
            Intent intent)
        {
            List<KeyValueStorage> storages = new List<KeyValueStorage>();
            if (savedInstanceState != null)
            {
                storages.Add(new BundleKeyValueStorage(savedInstanceState));
            }

            if (intent != null)
            {
                storages.Add(new IntentKeyValueStorage(intent));
            }

            return new ChainedKeyValueStorage(storages);
        }
    }
}