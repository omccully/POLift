using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    using Model;

    public interface ICreateExerciseViewModel : IValueReturner<IExercise>
    {
        void EditExercise(IExercise exercise, bool name_input_enabled = false);

        void Reset();
    }
}
