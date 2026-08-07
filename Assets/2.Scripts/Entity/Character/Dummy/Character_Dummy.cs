using UnityEngine;

public class Character_Dummy : MonoBehaviour, IDamageable
{
    [Header("스텟")]
    [field: SerializeField] public float MaxHP { get; private set; } = 100f;
    [field: SerializeField] public float HP    { get; private set; }

    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        Debug.Log($"{attackerOwner.name}의 {attacker.name}으로 {damage} 데미지 받음");
    }

}
