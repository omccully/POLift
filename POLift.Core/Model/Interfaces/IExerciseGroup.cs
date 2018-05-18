using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public interface IExerciseGroup
    {
        string Name { get; set; }
        string ExerciseIDs { get; set; }
        
        string Category { get; set; }

        int Usage { get; set; }
    }
}
