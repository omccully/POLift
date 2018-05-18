using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Model
{
    using Service;

    public interface IExercise : IExerciseGroup, IIdentifiable, IDeletable, IDatabaseObject
    {
        //string Name { get; set; }

        int MaxRepCount { get; set; }

        float WeightIncrement { get; set; }

        int RestPeriodSeconds { get; set; }

        int ConsecutiveSetsForWeightIncrease { get; set; }

        int PlateMathID { get; set; }

        //string Category { get; set; }

        IPlateMath PlateMath { get; set; }

        //int Usage { get; set; }

        string ShortDetails { get; }

        string CondensedDetails { get; }

        float NextWeight { get; }

        ExerciseDifficulty GetDifficultyRecord();

        bool RefreshUsage();

        int SucceedsInARow(int check_count = 0);
    }
}