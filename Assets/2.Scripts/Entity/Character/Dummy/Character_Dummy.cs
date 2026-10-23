using UnityEngine;

public class Character_Dummy : MonoBehaviour, IDamageable
{
    [Header("스텟")]
    [Tooltip("체력 (Max = 최대 체력, Current = 현재 체력)")]
    public FStat HP = new FStat { Max = 100f };

    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        Debug.Log($"{attackerOwner.name}의 {attacker.name}으로 {damage} 데미지 받음");
    }

}
