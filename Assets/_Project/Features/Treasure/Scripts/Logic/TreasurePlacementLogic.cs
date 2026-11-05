using System;
using System.Collections.Generic;

namespace Project.Treasure
{
    public sealed class TreasurePlacementLogic
    {
        public int SelectChest(int[] biomes, double[] weights, bool[] chestPoints, int biomeCount, int keyCount, Random random)
        {
            Validate(biomes, weights, biomeCount, keyCount, random);
            if (chestPoints == null || chestPoints.Length != biomes.Length)
                throw new ArgumentException("Chest flags must match the candidate list.");
            var candidates = new List<int>();
            for (int chest = 0; chest < chestPoints.Length; chest++)
            {
                if (!chestPoints[chest]) continue;
                var counts = new int[biomeCount];
                int total = 0;
                for (int point = 0; point < biomes.Length; point++)
                {
                    if (point == chest || weights[point] == 0) continue;
                    counts[biomes[point]]++;
                    total++;
                }
                if (total >= keyCount && Array.TrueForAll(counts, count => count > 0)) candidates.Add(chest);
            }
            if (candidates.Count == 0) throw new ArgumentException("No chest point permits the required keys and biome coverage.");
            return candidates[random.Next(candidates.Count)];
        }

        // The returned indices refer to the caller's candidate list, which is never mutated.
        public int[] SelectKeys(int[] biomes, double[] weights, int biomeCount, int keyCount, Random random)
        {
            Validate(biomes, weights, biomeCount, keyCount, random);
            var remaining = new List<int>();
            for (int index = 0; index < biomes.Length; index++)
            {
                if (weights[index] > 0) remaining.Add(index);
            }
            if (remaining.Count < keyCount) throw new ArgumentException("Not enough positive-weight key points.");
            var result = new List<int>();
            for (int biome = 0; biome < biomeCount; biome++)
            {
                var candidates = remaining.FindAll(index => biomes[index] == biome);
                if (candidates.Count == 0) throw new ArgumentException("Each biome needs a positive-weight key point.");
                int selected = Draw(candidates, weights, random);
                result.Add(selected);
                remaining.Remove(selected);
            }
            while (result.Count < keyCount)
            {
                int selected = Draw(remaining, weights, random);
                result.Add(selected);
                remaining.Remove(selected);
            }
            return result.ToArray();
        }

        private static void Validate(int[] biomes, double[] weights, int biomeCount, int keyCount, Random random)
        {
            if (biomes == null || weights == null || random == null) throw new ArgumentNullException();
            if (biomeCount < 1 || keyCount < biomeCount || biomes.Length != weights.Length)
                throw new ArgumentException("Require at least one key per biome and matching candidate arrays.");
            for (int index = 0; index < biomes.Length; index++)
                if (biomes[index] < 0 || biomes[index] >= biomeCount || double.IsNaN(weights[index]) ||
                    double.IsInfinity(weights[index]) || weights[index] < 0)
                    throw new ArgumentException("Invalid biome or weight.");
        }

        private static int Draw(List<int> candidates, double[] weights, Random random)
        {
            // Scaling avoids overflow even when several very large weights are configured.
            double maximum = 0;
            foreach (int index in candidates) maximum = Math.Max(maximum, weights[index]);
            double total = 0;
            foreach (int index in candidates) total += weights[index] / maximum;
            double draw = random.NextDouble() * total;
            foreach (int index in candidates)
            {
                draw -= weights[index] / maximum;
                if (draw < 0) return index;
            }
            return candidates[candidates.Count - 1];
        }
    }
}
