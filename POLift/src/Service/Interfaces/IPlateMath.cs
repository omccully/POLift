using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Service
{
    public interface IPlateMath
    {
        int BarWeight { get; }

        bool SplitWeights { get; }

        Dictionary<float, int> CalculateTotalPlateCounts(int weight);

        string PlateCountsToString(int weight);
    }
}