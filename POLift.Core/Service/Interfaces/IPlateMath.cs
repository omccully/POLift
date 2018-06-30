using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Service
{
    public interface IPlateMath
    {
        float BarWeight { get; }

        bool SplitWeights { get; }

        Dictionary<float, int> CalculateTotalPlateCounts(float weight, bool one_side_if_split = false);

        string PlateCountsToString(float weight, bool one_side_if_split = false);
    }
}