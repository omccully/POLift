using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public abstract class KeyValueStorage
    {
        public abstract KeyValueStorage SetValue(string key, string val);

        public abstract string GetString(string key, string default_val = null);

        public virtual KeyValueStorage SetValue(string key, int val)
        {
            return SetValue(key, val.ToString());
        }

        public virtual int GetInteger(string key, int default_val = 0)
        {
            try
            {
                return Int32.Parse(GetString(key));
            }
            catch
            {
                return default_val;
            }
        }

        public virtual KeyValueStorage SetValue(string key, bool val)
        {
            return SetValue(key, val.ToString());
        }

        public virtual bool GetBoolean(string key, bool default_val = false)
        {
            int stored_int = GetInteger(key, -1);
            if(stored_int == -1) return default_val;
            return (stored_int != 0);
        }

        public virtual KeyValueStorage SetValue(string key, int[] val)
        {
            return SetValue(key, val.ToIDString());
        }

        public virtual int[] GetIntArray(string key, int[] default_val = null)
        {
            string str = GetString(key, null);

            if (str == null) return default_val;

            return str.ToIDIntegers();
        }

        public virtual KeyValueStorage SetValue(string key, float val)
        {
            return SetValue(key, val.ToString());
        }

        public virtual float GetFloat(string key, float default_val = 0.0f)
        {
            string str = GetString(key);

            if (str == null) return default_val;
            return Single.Parse(str);
        }
    }
}
