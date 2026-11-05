using NUnit.Framework;
using Project.Treasure;

public sealed class TreasureDirectionLogicTests
{
    [Test]
    public void FindsNearestAvailableKeyAndKeepsFirstOnTie()
    {
        Assert.That(TreasureDirectionLogic.FindNearest(new double[] { 20, double.PositiveInfinity, 4, 4 }), Is.EqualTo(2));
        Assert.That(TreasureDirectionLogic.FindNearest(new[] { double.PositiveInfinity }), Is.EqualTo(-1));
        Assert.That(TreasureDirectionLogic.FindNearest(new double[0]), Is.EqualTo(-1));
    }

    [TestCase(0.5, 0.5)]
    [TestCase(0, 1)]
    [TestCase(1, 0)]
    public void VisibleTargetHidesMarker(double x, double y)
    {
        Assert.That(TreasureDirectionLogic.TryGetMarker(x, y, 1, 1920, 1080, 50, out _, out _, out _), Is.False);
    }

    [TestCase(2, 0.5, 1870, 540, 0)]
    [TestCase(-1, 0.5, 50, 540, 180)]
    [TestCase(0.5, 2, 960, 1030, 90)]
    [TestCase(0.5, -1, 960, 50, -90)]
    public void OutsideTargetUsesCorrectEdgeAndRotation(double vx, double vy, double expectedX, double expectedY, double expectedAngle)
    {
        Assert.That(TreasureDirectionLogic.TryGetMarker(vx, vy, 1, 1920, 1080, 50, out var x, out var y, out var angle), Is.True);
        Assert.That(x, Is.EqualTo(expectedX).Within(0.001));
        Assert.That(y, Is.EqualTo(expectedY).Within(0.001));
        Assert.That(angle, Is.EqualTo(expectedAngle).Within(0.001));
    }

    [TestCase(1920, 1080)]
    [TestCase(1080, 1920)]
    [TestCase(2560, 1080)]
    public void DiagonalMarkerStaysInsideInsetAtDifferentAspectRatios(double width, double height)
    {
        Assert.That(TreasureDirectionLogic.TryGetMarker(2, 2, 1, width, height, 50, out var x, out var y, out var angle), Is.True);
        Assert.That(x, Is.InRange(50, width - 50));
        Assert.That(y, Is.InRange(50, height - 50));
        Assert.That(angle, Is.InRange(0, 90));
    }

    [Test]
    public void BehindCameraAndTinyViewportAreHandled()
    {
        Assert.That(TreasureDirectionLogic.TryGetMarker(0.5, 0.5, -1, 1920, 1080, 50, out var x, out var y, out _), Is.True);
        Assert.That(x, Is.EqualTo(960));
        Assert.That(y, Is.EqualTo(1030));
        Assert.That(TreasureDirectionLogic.TryGetMarker(2, 2, 1, 50, 50, 50, out _, out _, out _), Is.False);
    }
}
