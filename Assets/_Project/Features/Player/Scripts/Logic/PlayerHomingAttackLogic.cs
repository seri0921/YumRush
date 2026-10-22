public static class HomingAttackLogic
{
    public static bool IsCloser(float candidateDistanceSquared, float currentDistanceSquared, float rangeSquared)
    {
        return candidateDistanceSquared <= rangeSquared && candidateDistanceSquared < currentDistanceSquared;
    }
}
