using System;
using System.Linq;
using NUnit.Framework;
using Project.Treasure;

public sealed class TreasureLogicTests
{
    [Test]
    public void ChestSelectionPreservesKeyCapacityAndNeverSharesAKeyPoint()
    {
        var logic = new TreasurePlacementLogic();
        int[] biomes = { 0, 1, 2, 3, 3 };
        double[] weights = { 1, 1, 1, 1, 1 };
        for (int seed = 0; seed < 50; seed++)
        {
            int chest = logic.SelectChest(biomes, weights, new[] { true, true, true, true, true }, 4, 4, new Random(seed));
            Assert.That(chest, Is.InRange(3, 4));
            var keyWeights = (double[])weights.Clone();
            keyWeights[chest] = 0;
            Assert.That(logic.SelectKeys(biomes, keyWeights, 4, 4, new Random(seed)), Has.No.Member(chest));
        }
        Assert.Throws<ArgumentException>(() => logic.SelectChest(biomes, weights, new[] { true, false, false, false, false }, 4, 4, new Random()));
    }

    [Test]
    public void SelectionCoversAllBiomesWithoutDuplicatesAndPreservesInput()
    {
        int[] biomes = { 0, 0, 1, 1, 2, 2, 3, 3 };
        double[] weights = { 1, 2, 1, 2, 1, 2, 1, 2 };
        var original = (double[])weights.Clone();
        for (int seed = 0; seed < 100; seed++)
        {
            var selected = new TreasurePlacementLogic().SelectKeys(biomes, weights, 4, 6, new Random(seed));
            Assert.That(selected.Length, Is.EqualTo(6));
            Assert.That(selected.Distinct().Count(), Is.EqualTo(6));
            Assert.That(selected.Select(index => biomes[index]).Distinct().Count(), Is.EqualTo(4));
        }
        Assert.That(weights, Is.EqualTo(original));
    }

    [Test]
    public void ZeroWeightsAreExcludedAndSeedIsReproducible()
    {
        int[] biomes = { 0, 0, 1, 2, 3 };
        double[] weights = { 0, 1, 1, 1, 1 };
        var logic = new TreasurePlacementLogic();
        var selected = logic.SelectKeys(biomes, weights, 4, 4, new Random(1));
        Assert.That(selected, Has.No.Member(0));
        Assert.That(selected, Is.EqualTo(logic.SelectKeys(biomes, weights, 4, 4, new Random(1))));
    }

    [Test]
    public void WeightedSelectionFavorsLargerWeights()
    {
        int highWeightSelections = 0;
        for (int seed = 0; seed < 1000; seed++)
        {
            var selected = new TreasurePlacementLogic().SelectKeys(new[] { 0, 0 }, new double[] { 1, 9 }, 1, 1, new Random(seed));
            if (selected[0] == 1) highWeightSelections++;
        }
        Assert.That(highWeightSelections, Is.InRange(850, 950));
    }

    [Test]
    public void InvalidCapacityAndMissingBiomesAreRejected()
    {
        var logic = new TreasurePlacementLogic();
        Assert.Throws<ArgumentException>(() => logic.SelectKeys(new[] { 0, 1, 2, 3 }, new double[] { 1, 1, 1, 1 }, 4, 3, new Random()));
        Assert.Throws<ArgumentException>(() => logic.SelectKeys(new[] { 0, 1, 2, 3 }, new double[] { 1, 1, 1, 0 }, 4, 4, new Random()));
        Assert.Throws<ArgumentException>(() => logic.SelectKeys(new[] { 0, 0, 1, 2 }, new double[] { 1, 1, 1, 1 }, 4, 4, new Random()));
        Assert.Throws<ArgumentException>(() => logic.SelectKeys(new[] { 0 }, new[] { double.NaN }, 1, 1, new Random()));
    }

    [Test]
    public void ChestRequiresEveryUniqueKeyAndOpensOnlyOnce()
    {
        var progress = new TreasureProgressLogic(4);
        Assert.That(progress.TryOpen(), Is.False);
        Assert.That(progress.TryCollect(0), Is.True);
        Assert.That(progress.TryCollect(0), Is.False);
        Assert.That(progress.CollectedKeys, Is.EqualTo(1));
        progress.TryCollect(1);
        progress.TryCollect(2);
        Assert.That(progress.TryOpen(), Is.False);
        progress.TryCollect(3);
        Assert.That(progress.CanOpen, Is.True);
        Assert.That(progress.TryOpen(), Is.True);
        Assert.That(progress.TryOpen(), Is.False);
        Assert.That(progress.IsOpened, Is.True);
        Assert.Throws<ArgumentOutOfRangeException>(() => progress.TryCollect(4));
    }
}

