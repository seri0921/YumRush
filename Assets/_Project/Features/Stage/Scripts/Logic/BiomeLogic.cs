using System;
using System.Collections.Generic;

/// <summary>Selects distinct biome IDs in placement order without changing the candidates.</summary>
public sealed class BiomeLogic
{
    public string[] CreateLayout(IReadOnlyList<string> candidateIds, int slotCount, Random random)
    {
        if (candidateIds == null)
            throw new ArgumentNullException(nameof(candidateIds));
        if (slotCount <= 0 || slotCount > candidateIds.Count)
            throw new ArgumentOutOfRangeException(nameof(slotCount), "Provide at least one candidate per Slot.");
        if (random == null)
            throw new ArgumentNullException(nameof(random));

        var candidates = new string[candidateIds.Count];
        var uniqueIds = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < candidateIds.Count; index++)
        {
            string id = candidateIds[index];
            if (string.IsNullOrWhiteSpace(id) || !uniqueIds.Add(id))
                throw new ArgumentException("Biome IDs must be nonempty and unique.", nameof(candidateIds));
            candidates[index] = id;
        }

        var layout = new string[slotCount];
        for (int index = 0; index < slotCount; index++)
        {
            int chosenIndex = random.Next(index, candidates.Length);
            string chosenId = candidates[chosenIndex];
            candidates[chosenIndex] = candidates[index];
            candidates[index] = chosenId;
            layout[index] = chosenId;
        }
        return layout;
    }
}
