using System;
using UnityEngine;

/// <summary>
/// Unity側の被弾受付。被弾の可否と動作の中断はMachineに委譲する。
/// </summary>
internal sealed class PlayerDamageReceiver
{
    private readonly PlayerStateMachine machine;
    private readonly PlayerPhysicsMotor motor;
    private readonly PlayerMovementConfig config;

    private readonly PlayerInput input;

    // ホーミング機能を搭載しないプレイヤーではnull。
    private readonly PlayerHomingAttack homing;

    public PlayerDamageReceiver(PlayerStateMachine machine, PlayerPhysicsMotor motor,
        PlayerMovementConfig config, PlayerInput input, PlayerHomingAttack homing)
    {
        this.machine = machine ?? throw new ArgumentNullException(nameof(machine));
        this.motor = motor ?? throw new ArgumentNullException(nameof(motor));
        this.config = config != null ? config : throw new ArgumentNullException(nameof(config));
        this.input = input ?? throw new ArgumentNullException(nameof(input));
        this.homing = homing;
    }

    public bool Receive()
    {
        if (!machine.TryReceiveHit(config.EnemyControlLockDuration, config.EnemyInvulnerabilityDuration))
            return false;
        input.ClearRequests();
        homing?.ClearTarget();
        motor.RestoreGravity();
        motor.ApplyVelocity(Vector2.down * config.EnemyKnockbackSpeed);
        return true;
    }
}