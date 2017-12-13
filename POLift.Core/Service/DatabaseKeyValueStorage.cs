using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Service
{
    using Model;

    public class DatabaseKeyValueStorage : KeyValueStorage
    {
        IPOLDatabase Database;

        public DatabaseKeyValueStorage(IPOLDatabase database)
        {
            this.Database = database;
        }

        public override string GetString(string key, string default_val = null)
        {
            ValueLookup lookup = ValueObjectFromKey(key);
            if (lookup == null)
            {
                return default_val;
            }
            else
            {
                return lookup.ValueString;
            }
        }
            
        public override void SetValue(string key, string val)
        {
            ValueLookup value = new ValueLookup();
            value.LookupKey = key;
            value.ValueString = val;
            Database.InsertOrReplace(value);
        }

        public override int GetInteger(string key, int default_val = 0)
        {
            ValueLookup lookup = ValueObjectFromKey(key);
            if(lookup == null)
            {
                return default_val;
            }
            else
            {
                return lookup.ValueInt;
            }
        }

        public override void SetValue(string key, int val)
        {
            base.SetValue(key, val);
        }

        ValueLookup ValueObjectFromKey(string key)
        {
            return Database.Query<ValueLookup>(
                "SELECT * FROM ValueLookup WHERE LookupKey = ?", key)
                .FirstOrDefault();
        
    }
    }
}
