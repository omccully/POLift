using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using POLift.Core.Model;

namespace POLift.Core.ViewModel
{
    public interface IEditRoutineResultViewModel
    {
        event Action DoneEditing;

        IRoutineResult RoutineResult { get; set; }
    }
}
