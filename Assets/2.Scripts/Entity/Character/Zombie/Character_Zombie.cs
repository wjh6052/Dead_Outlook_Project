using UnityEngine;

// 좀비 캐릭터 베이스 클래스
// - IDamageable 구현 · HP/공격력 스텟 · 사망 처리
public class Character_Zombie : MonoBehaviour, IDamageable
{
    #region 스텟
    [Header("스텟")]
    [field: SerializeField] public float MaxHP        { get; private set; } = 100f;
    [field: SerializeField] public float HP           { get; private set; }
    [field: SerializeField] public float AttackDamage { get; private set; } = 10f;
    #endregion

    #region 컴포넌트
    public MeshComponent  MeshComp  { get; private set; }
    public ZombieAnimator ZombieAnim { get; private set; }
    #endregion

    private void Awake()
    {
        HP        = MaxHP;
        MeshComp  = GetComponent<MeshComponent>();
        ZombieAnim = GetComponent<ZombieAnimator>();
    }

    // IDamageable — 데미지를 받아 HP 감소, 0이 되면 사망 처리
    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        HP = Mathf.Max(HP - damage, 0f);
        Debug.Log($"[{name}] {attacker.name}에게 {damage} 데미지 — 남은 HP: {HP}/{MaxHP}");
        ZombieAnim.PlayHit();
        if (HP <= 0f) OnDeath();
    }

    // 사망 처리
    private void OnDeath()
    {
        Debug.Log($"좀비 사망");
        ZombieAnim.PlayDeath();
    }
}
