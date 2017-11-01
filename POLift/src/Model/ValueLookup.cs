using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite;

namespace POLift.Model
{
    class ValueLookup
    {
        [PrimaryKey]
        public string LookupKey { get; set; }

        public int ValueInt { get; set; }

        public string ValueString { get; set; }
    }
}