using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POLift.Core.Model;

namespace POLift.Core.ViewModel
{
    public interface ICreateRoutineViewModel : IValueReturner<IRoutine>
    {
        void EditRoutine(IRoutine routine, int locked_sets = 0);

        void Reset();

    }
}
