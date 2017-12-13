using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POLift.Core.Service;
using SQLite.Net.Attributes;

namespace POLift.Core.Model
{
    public class ValueLookup : IDatabaseObject
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey]
        public string LookupKey { get; set; }

        public int ValueInt { get; set; }

        public string ValueString { get; set; }
    }
}