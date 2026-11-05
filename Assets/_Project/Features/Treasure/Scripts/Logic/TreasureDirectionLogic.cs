using System;

namespace Project.Treasure
{
    public static class TreasureDirectionLogic
    {
        // Positive infinity denotes a collected, destroyed or unavailable key.
        public static int FindNearest(double[] squaredDistances)
        {
            int nearest = -1;
            double distance = double.PositiveInfinity;
            for (int index = 0; index < squaredDistances.Length; index++)
            {
                if (squaredDistances[index] >= distance) continue;
                distance = squaredDistances[index];
                nearest = index;
            }
            return nearest;
        }

        // Returns coordinates relative to the camera's pixel rectangle and a right-facing sprite angle.
        public static bool TryGetMarker(double viewportX, double viewportY, double depth,
            double width, double height, double inset, out double x, out double y, out double angle)
        {
            x = y = angle = 0;
            if (depth > 0 && viewportX >= 0 && viewportX <= 1 && viewportY >= 0 && viewportY <= 1)
                return false;
            double halfWidth = width * 0.5 - inset;
            double halfHeight = height * 0.5 - inset;
            if (halfWidth <= 0 || halfHeight <= 0) return false;
            double directionX = (viewportX - 0.5) * width;
            double directionY = (viewportY - 0.5) * height;
            if (depth <= 0)
            {
                directionX = -directionX;
                directionY = -directionY;
            }
            if (directionX == 0 && directionY == 0) directionY = 1;
            double scaleX = directionX == 0 ? double.PositiveInfinity : halfWidth / Math.Abs(directionX);
            double scaleY = directionY == 0 ? double.PositiveInfinity : halfHeight / Math.Abs(directionY);
            double scale = Math.Min(scaleX, scaleY);
            x = width * 0.5 + directionX * scale;
            y = height * 0.5 + directionY * scale;
            angle = Math.Atan2(directionY, directionX) * 180 / Math.PI;
            return true;
        }
    }
}
