using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POLift.Core.Service
{
    public class PlateMath : IPlateMath
    {
        static float[] imperial_no35 = { 2.5f, 5, 10, 25, 45 };
        static float[] imperial_35 = { 2.5f, 5, 10, 25, 35, 45 };

        static float[] metric = { 1.25f, 2.5f, 5, 10, 15, 20, 25 };

        public static readonly PlateMath ImperialBarbellAndPlatesNo35s = new PlateMath(imperial_no35, 45);
        public static readonly PlateMath ImperialBarbellAndPlatesWith35s = new PlateMath(imperial_35, 45);
        public static readonly PlateMath ImperialPlatesNo35s = new PlateMath(imperial_no35);
        public static readonly PlateMath ImperialPlatesWith35s = new PlateMath(imperial_35);
        public static readonly PlateMath ImperialPlatesNo35sNoSplit = new PlateMath(imperial_no35, 0, false);
        public static readonly PlateMath ImperialPlatesWith35sNoSplit = new PlateMath(imperial_35, 0, false);

        public static readonly PlateMath MetricBarbellAndPlates = new PlateMath(metric, 20);
        public static readonly PlateMath MetricPlates = new PlateMath(metric);
        public static readonly PlateMath MetricPlatesNoSplit = new PlateMath(metric, 0, false);

        // don't change the order of these EVER
        public static readonly PlateMath[] PlateMathTypes =
        {
            null,
            ImperialBarbellAndPlatesNo35s,
            ImperialBarbellAndPlatesWith35s,
            ImperialPlatesNo35s,
            ImperialPlatesWith35s,
            ImperialPlatesNo35sNoSplit,
            ImperialPlatesWith35sNoSplit,
            MetricBarbellAndPlates,
            MetricPlates,
            MetricPlatesNoSplit
        };


        float[] PlateWeights;
        public float BarWeight { get; private set; }
        public bool SplitWeights { get; private set; }

        public PlateMath(float[] plate_weights, float bar_weight = 0, bool split_weights = true)
        {
            PlateWeights = plate_weights.ToArray();
            Array.Sort(PlateWeights);
            BarWeight = bar_weight;
            SplitWeights = split_weights;
        }

        public override string ToString()
        {
            string plate_weights = String.Join(", ", PlateWeights);

            string result = "";

            if (BarWeight > 0) result += $"{BarWeight} bar and ";
            result += $"[{plate_weights}] plates";
            if (!SplitWeights) result += ", not split";

            return result;
        }

        public Dictionary<float, int> CalculateTotalPlateCounts(float weight)
        {
            weight -= BarWeight;

            if (SplitWeights)
            {

                return CalculatePlateCountsOneSide(weight / 2.0f).
                    // double all values
                    Select(f => new KeyValuePair<float, int>(f.Key, f.Value * 2))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return CalculatePlateCountsOneSide(weight);
        }

        public string PlateCountsToString(float weight)
        {
            return PlateCountsToString(CalculateTotalPlateCounts(weight));
        }

        static string PlateCountsToString(Dictionary<float, int> dict)
        {
            return String.Join(", ", dict.Select(kv => $"{kv.Key}x{kv.Value}"));
        }

        /// <summary>
        /// Plate counts without additional weight added
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="plate_weights"></param>
        /// <returns></returns>
        Dictionary<float, int> CalculatePlateCountsOneSide(float weight)
        {
            const float TOLERANCE = 0.01f;
            Dictionary<float, int> result = new Dictionary<float, int>();

            float remaining_weight = weight + TOLERANCE;

            for (int i = PlateWeights.Length - 1; i >= 0; i--)
            {
                if (PlateWeights[i] <= 0) throw new ArgumentOutOfRangeException("Plate weights must be positive");

                int plate_count = (int)(remaining_weight / PlateWeights[i]);

                if (plate_count == 0) continue;

                remaining_weight -= (plate_count * PlateWeights[i]);

                result[PlateWeights[i]] = plate_count;
            }

            return result;
        }
    }
}