using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    public class ChainedKeyValueStorage : KeyValueStorage
    {
        List<KeyValueStorage> storages;

        public ChainedKeyValueStorage(IEnumerable<KeyValueStorage> storages)
        {
            this.storages = new List<KeyValueStorage>(storages);
        }

        public override string GetString(string key, string default_val = null)
        {
            foreach(KeyValueStorage kvs in storages)
            {
                string result = kvs.GetString(key, null);
                if (result != null) return result;
            }

            return default_val;
        }

        public override KeyValueStorage SetValue(string key, string val)
        {
            return storages[0].SetValue(key, val);
        }

        public override bool GetBoolean(string key, bool default_val = false)
        {
            foreach (KeyValueStorage kvs in storages)
            {
                bool result = kvs.GetBoolean(key, default_val);
                if (result != default_val) return result;
            }

            return default_val;
        }

        public override KeyValueStorage SetValue(string key, bool val)
        {
            return storages[0].SetValue(key, val);
        }

        public int InvalidInteger = -1;
        public override int GetInteger(string key, int default_val = 0)
        {
            foreach (KeyValueStorage kvs in storages)
            {
                int result = kvs.GetInteger(key, InvalidInteger);
                if (result != InvalidInteger) return result;
            }

            return default_val;
        }

        public override KeyValueStorage SetValue(string key, int val)
        {
            return storages[0].SetValue(key, val);
        }

        public override int[] GetIntArray(string key, int[] default_val = null)
        {
            foreach (KeyValueStorage kvs in storages)
            {
                int[] result = kvs.GetIntArray(key);
                if (result != null) return result;
            }

            return default_val;
        }

        public override KeyValueStorage SetValue(string key, int[] val)
        {
            return storages[0].SetValue(key, val);
        }

    }
}
