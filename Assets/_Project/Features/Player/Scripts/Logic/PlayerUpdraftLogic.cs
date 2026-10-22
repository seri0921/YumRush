using System;

public class PlayerUpdraftLogic
{ 
    private float _phase;
    private float _previousPhase;
    private int _lastJumpCycle = -1;
    private int _cycle;
    private bool _windJumpConsumed;
    
    public float Phase => _phase;
    public float LiftVelocity { get; private set; }
    public bool CanWindJump => _lastJumpCycle != _cycle && !_windJumpConsumed;
    
    public void EnterWind(float phaseAtEntry = 0f)
    {
        _phase = phaseAtEntry;
        _previousPhase = phaseAtEntry;
        _cycle = 0;
        _lastJumpCycle = -1;
        _windJumpConsumed = false;
        LiftVelocity = 0f;
    }
    
    public (float X, float Y) StepCapture(float positionX, float positionY,
        float velocityX, float velocityY, float centerX, float centerY,
        float deltaTime, float maximumSpeed, float acceleration, float arrivalDistance)
    {
        float dx = centerX - positionX;
        float dy = centerY - positionY;
        float distance = (float)Math.Sqrt(dx * dx + dy * dy);
        float speed = (float)Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
        if (distance <= arrivalDistance && speed <= acceleration * deltaTime)
        {
            return (0f, 0f);
        }

        float desiredSpeed = Math.Min(maximumSpeed, (float)Math.Sqrt(2f * acceleration * distance));
        float desiredX = distance > 0f ? dx / distance * desiredSpeed : 0f;
        float desiredY = distance > 0f ? dy / distance * desiredSpeed : 0f;
        float changeX = desiredX - velocityX;
        float changeY = desiredY - velocityY;
        float changeLength = (float)Math.Sqrt(changeX * changeX + changeY * changeY);
        float maximumChange = acceleration * deltaTime;
        if (changeLength > maximumChange)
        {
            changeX *= maximumChange / changeLength;
            changeY *= maximumChange / changeLength;
        }
        return (velocityX + changeX, velocityY + changeY);
    }
    
    public void LeaveWind()
    {
        LiftVelocity = 0f;
    }
    
    public float StepWind(float deltaTime, float period, float amplitude)
    {
        _previousPhase = _phase;
        _phase += deltaTime * 2f * (float)Math.PI / period;
        if ((int)(_phase / (2f * (float)Math.PI)) > (int)(_previousPhase / (2f * (float)Math.PI)))
            _cycle++;
        LiftVelocity = amplitude * 2f * (float)Math.PI / period * (float)Math.Cos(_phase);
        return amplitude * (float)Math.Sin(_phase);
    }
    
    public bool TryWindJump(float baseSpeed, float boostMultiplier, out float jumpSpeed)
    {
        jumpSpeed = 0f;
        if (!CanWindJump)
            return false;
        jumpSpeed = baseSpeed + Math.Max(0f, LiftVelocity) * boostMultiplier;
        _lastJumpCycle = _cycle;
        _windJumpConsumed = true;
        return true;
    }
    
    public void ResumeFloating() => _windJumpConsumed = false;
    
    public static bool HasCompletedCapture(float dx, float dy, float velocityX, float velocityY,
        float stopSpeed, float arrivalDistance)
    {
        return dx * dx + dy * dy <= arrivalDistance * arrivalDistance &&
               velocityX * velocityX + velocityY * velocityY <= stopSpeed * stopSpeed;
    }
    
    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    
}
