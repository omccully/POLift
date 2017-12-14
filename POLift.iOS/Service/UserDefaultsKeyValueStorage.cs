using System;
using System.Collections.Generic;
using System.Text;

using Foundation;
using UIKit;

using POLift.Core.Service;

namespace POLift.iOS.Service
{
    class UserDefaultsKeyValueStorage : KeyValueStorage
    {
        NSUserDefaults Defaults;

        public UserDefaultsKeyValueStorage(NSUserDefaults defaults = null)
        {
            if(defaults == null)
            {
                this.Defaults = NSUserDefaults.StandardUserDefaults;
            }
            else
            {
                this.Defaults = defaults;
            }
        }

        public override string GetString(string key, string default_val = null)
        {
            if (Defaults[key] == null) return default_val;
            return Defaults.StringForKey(key);
        }

        public override KeyValueStorage SetValue(string key, string val)
        {
            Defaults.SetString(val, key);

            return this;
        }

        public override KeyValueStorage SetValue(string key, bool val)
        {
            Defaults.SetBool(val, key);
            return this;
        }

        public override bool GetBoolean(string key, bool default_val = false)
        {
            if (Defaults[key] == null) return default_val;
            return Defaults.BoolForKey(key);
        }

        public override int GetInteger(string key, int default_val = 0)
        {
            if (Defaults[key] == null) return default_val;
            return (int)Defaults.IntForKey(key);
        }

        public override KeyValueStorage SetValue(string key, int val)
        {
            Defaults.SetInt(val, key);
            return this;
        }
    }
}
