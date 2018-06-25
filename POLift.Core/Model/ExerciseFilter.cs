using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.Model
{
    public enum ExerciseFilterVariable
    {
        MaxRepCount,
        RestPeriodSeconds
    }

    public enum ExerciseFilterOperators
    {
        LessThan,
        GreaterThan,
        EqualTo,
        NotEqualTo
    }

    public class ExerciseFilter
    {
        public ExerciseFilterVariable Variable { get; set; }
        public ExerciseFilterOperators Operator { get; set; }
        public int Value { get; set; }

        public ExerciseFilter(ExerciseFilterVariable var,
            ExerciseFilterOperators op, int val)
        {
            this.Variable = var;
            this.Operator = op;
            this.Value = val;
        }

        public bool Accepts(IExercise test_ex)
        {
            int test_val = GetTestValue(test_ex);

            switch(Operator)
            {
                case ExerciseFilterOperators.EqualTo:
                    return test_val == Value;
                case ExerciseFilterOperators.NotEqualTo:
                    return test_val != Value;
                case ExerciseFilterOperators.GreaterThan:
                    return test_val > Value;
                case ExerciseFilterOperators.LessThan:
                    return test_val < Value;
            }

            return false;
        }

        int GetTestValue(IExercise test_ex)
        {
            switch (Variable)
            {
                case ExerciseFilterVariable.MaxRepCount:
                    return test_ex.MaxRepCount;
                case ExerciseFilterVariable.RestPeriodSeconds:
                    return test_ex.RestPeriodSeconds; 
            }

            return 0;
        }

        public override string ToString()
        {
            return $"{Variable}{Operator}{Value}";
        }
    }
}
