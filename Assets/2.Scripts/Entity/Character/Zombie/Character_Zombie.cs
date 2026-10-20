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

    [Header("이동")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = 1f; // 걷기 속도 (Alert · Chase)
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
        if (HP <= 0f) return; // 이미 사망 — 데미지 · 피격 · 사망 처리 모두 무시

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

    

    // 사망 처리
    private void OnDeath()
    {
        Debug.Log($"좀비 사망");
        ZombieAnim.PlayDeath();
        AICtrl.OnDeath();
    }

    // 대상 위치를 향해 즉시 회전 — 높이 차이는 무시하고 수평으로만 회전
    public void LookAt(Vector3 targetPosition)
    {
        Vector3 dir = Vector3.ProjectOnPlane(targetPosition - transform.position, Vector3.up);
        if (dir.sqrMagnitude < 0.0001f) return; // 같은 위치면 방향을 정할 수 없음

        transform.rotation = Quaternion.LookRotation(dir);
    }


    #region 애니메이션 이벤트

    // 공격 종료 — 애니메이션 이벤트로 호출
    public void EndAttack()
    {
        AICtrl.OnEndAttack();
    }

    // 물기 — Zombie_Bite 애니메이션 이벤트로 호출 (이빨이 닿는 프레임)
    public void OnBite()
    {
        if (AICtrl.Target == null) return; // 물 대상 없음

        AICtrl.Target.TakeDamage(AttackDamage, gameObject, gameObject); // 공격력만큼 플레이어에게 데미지
    }

    #endregion
}
