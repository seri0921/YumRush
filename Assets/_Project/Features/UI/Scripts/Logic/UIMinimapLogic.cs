using System;

public readonly struct MinimapPosition
{
    public readonly float X;
    public readonly float Y;

    public MinimapPosition(float x, float y)
    {
        X = x;
        Y = y;
    }
}

public sealed class UIMinimapLogic
{
    public MinimapPosition Evaluate(float playerX, float playerY,
        float minX, float minY, float width, float height)
    {
        ValidateBounds(minX, minY, width, height);
        if (!IsFinite(playerX) || !IsFinite(playerY))
            throw new ArgumentOutOfRangeException(nameof(playerX), "Player coordinates must be finite.");

        return new MinimapPosition(
            (float)Math.Clamp(((double)playerX - minX) / width, 0d, 1d),
            (float)Math.Clamp(((double)playerY - minY) / height, 0d, 1d));
    }

    public static void ValidateBounds(float minX, float minY, float width, float height)
    {
        if (!IsFinite(minX) || !IsFinite(minY) || !IsFinite(width) ||
            !IsFinite(height) || width <= 0f || height <= 0f)
            throw new ArgumentOutOfRangeException(nameof(width),
                "Stage bounds must be finite with positive width and height.");
    }

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
