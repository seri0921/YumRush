using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの入力を担当
/// </summary>
internal sealed class PlayerInput : IDisposable
{
    private readonly InputAction move = new InputAction("Move", InputActionType.Value);
    private readonly InputAction jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
    private readonly InputAction attack = new InputAction("Attack", InputActionType.Button, "<Keyboard>/j");
    private readonly InputAction dash = new InputAction("AirDash", InputActionType.Button, "<Keyboard>/k");
    private int operationLockCount;
    private bool isEnabled;
    private bool isDisposed;

    public Vector2 Direction { get; private set; }
    public bool JumpQueued { get; private set; }
    public bool AttackQueued { get; private set; }
    public bool DashQueued { get; private set; }

    public PlayerInput()
    {
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        move.AddBinding("<Gamepad>/leftStick");
        jump.AddBinding("<Gamepad>/buttonSouth");
        attack.AddBinding("<Gamepad>/buttonEast");
        dash.AddBinding("<Gamepad>/buttonNorth");
        dash.AddBinding("<Gamepad>/buttonWest");
    }

    public void Enable()
    {
        isEnabled = true;
        move.Enable();
        jump.Enable();
        attack.Enable();
        dash.Enable();
    }

    public void Disable()
    {
        isEnabled = false;
        move.Disable();
        jump.Disable();
        attack.Disable();
        dash.Disable();
        Direction = Vector2.zero;
        ClearRequests();
    }

    public void Sample(bool locked, bool movementLocked)
    {
        locked |= operationLockCount > 0 || !isEnabled || SceneTransitionPresenter.IsTransitioning;
        Direction = locked || movementLocked ? Vector2.zero : move.ReadValue<Vector2>();
        if (locked)
        {
            ClearRequests();
            return;
        }

        AttackQueued |= attack.WasPressedThisFrame();
        if (movementLocked)
        {
            ClearJump();
            ClearDash();
            return;
        }

        JumpQueued |= jump.WasPressedThisFrame();
        DashQueued |= dash.WasPressedThisFrame();
    }

    public void ClearJump() => JumpQueued = false;
    public void ClearDash() => DashQueued = false;

    public void ClearRequests()
    {
        JumpQueued = false;
        AttackQueued = false;
        DashQueued = false;
    }

    public void Dispose()
    {
        isDisposed = true;
        isEnabled = false;
        move.Dispose();
        jump.Dispose();
        attack.Dispose();
        dash.Dispose();
    }

    public IDisposable LockOperation()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(PlayerInput));

        operationLockCount++;
        Direction = Vector2.zero;
        ClearRequests();
        return new OperationLock(this);
    }

    private sealed class OperationLock : IDisposable
    {
        // null means this handle has already been released.
        private PlayerInput owner;

        public OperationLock(PlayerInput owner) => this.owner = owner;

        public void Dispose()
        {
            if (owner == null)
                return;

            owner.operationLockCount--;
            owner = null;
        }
    }
}
