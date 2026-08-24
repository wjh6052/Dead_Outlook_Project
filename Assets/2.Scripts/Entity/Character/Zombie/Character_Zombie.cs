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
    public MeshComponent       MeshComp   { get; private set; }
    public ZombieAnimator      ZombieAnim { get; private set; }
    public AIZombieController  AICtrl     { get; private set; }
    #endregion

    private void Awake()
    {
        HP         = MaxHP;
        MeshComp   = GetComponent<MeshComponent>();
        ZombieAnim = GetComponent<ZombieAnimator>();
        AICtrl     = GetComponent<AIZombieController>();
    }

    // IDamageable — 데미지를 받아 HP 감소, 0이 되면 사망 처리
    public void TakeDamage(float damage, GameObject attacker, GameObject attackerOwner)
    {
        HP = Mathf.Max(HP - damage, 0f);
        Debug.Log($"[{name}] {attacker.name}에게 {damage} 데미지 — 남은 HP: {HP}/{MaxHP}");
        if (HP <= 0f) OnDeath();
        else          OnHit(attacker);
    }

    // 피격 처리
    private void OnHit(GameObject attacker)
    {
        ZombieAnim.PlayHit();
        AICtrl.OnHit(attacker);
    }

    // 피격 종료 — 애니메이션 이벤트로 호출
    public void EndHit()
    {
        AICtrl.OnEndHit();
    }

    // 공격 종료 — 애니메이션 이벤트로 호출
    public void EndAttack()
    {
        AICtrl.OnEndAttack();
    }

    // 사망 처리
    private void OnDeath()
    {
        Debug.Log($"좀비 사망");
        ZombieAnim.PlayDeath();
        AICtrl.OnDeath();
    }
}
