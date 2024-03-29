﻿using System;
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
            if (database == null) throw new ArgumentNullException("Database is null");
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
            
        public override KeyValueStorage SetValue(string key, string val)
        {
            ValueLookup value = new ValueLookup();
            value.LookupKey = key;
            value.ValueString = val;
            Database.InsertOrReplace(value);

            return this;
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

        public override KeyValueStorage SetValue(string key, int val)
        {
            ValueLookup value = new ValueLookup();
            value.LookupKey = key;
            value.ValueInt = val;
            Database.InsertOrReplace(value);

            return this;
        }

        ValueLookup ValueObjectFromKey(string key)
        {
            return Database.Query<ValueLookup>(
                "SELECT * FROM ValueLookup WHERE LookupKey = ?", key)
                .FirstOrDefault();
        }
    }
}
