using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    using Service;

    public interface IDatabaseObject
    {
        IPOLDatabase Database { get; set; }

    }
}