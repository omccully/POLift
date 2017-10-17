using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Model
{
    public interface IDeletable : IIdentifiable
    {
        bool Deleted { get; set; }
    }
}