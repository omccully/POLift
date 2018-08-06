using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public class ExerciseWithResult
    {
        public IExercise Exercise { get; set; }
        public IExerciseResult Result { get; set; }
        public int ExpectedWeight { get; set; }
    }
}
