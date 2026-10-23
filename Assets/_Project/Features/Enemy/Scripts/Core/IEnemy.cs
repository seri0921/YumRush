using System;
using UnityEngine;

public interface IEnemy
{
    bool IsDefeated { get; }
    void ReceiveAttack();

}