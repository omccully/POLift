using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    public interface IDeletable : IIdentifiable
    {
        bool Deleted { get; set; }
    }
}