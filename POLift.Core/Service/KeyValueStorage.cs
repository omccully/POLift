using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public abstract class KeyValueStorage
    {
        public abstract void SetValue(string key, string val);

        public abstract string GetString(string key, string default_val = null);

        public virtual void SetValue(string key, int val)
        {
            SetValue(key, val.ToString());
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

        public virtual void SetValue(string key, bool val)
        {
            SetValue(key, val.ToString());
        }

        public virtual bool GetBoolean(string key, bool default_val = false)
        {
            int stored_int = GetInteger(key, -1);
            if(stored_int == -1) return default_val;
            return (stored_int != 0);
        }


    }
}
