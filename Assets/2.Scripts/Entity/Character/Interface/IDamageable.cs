using UnityEngine;

public interface IDamageable
{
    float HP    { get; }
    float MaxHP { get; }
    void  TakeDamage(float damage, GameObject attacker, GameObject attackerOwner);
}
