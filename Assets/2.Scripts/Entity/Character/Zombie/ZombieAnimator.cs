using UnityEngine;

// 좀비 애니메이터 컨트롤러
// - EZombieState에 따라 Speed 파라미터 갱신
// - 공격·피격·사망 트리거 제공
public class ZombieAnimator : MonoBehaviour
{
    #region 설정
    [Tooltip("Speed 파라미터 보간 시간 (초) — 값이 클수록 이동↔대기 전환이 부드러워짐")]
    [SerializeField] private float MoveDampTime = 0.15f;
    #endregion

    #region 내부 상태
    private Animator Anim;
    private float    SmoothedSpeed;
    #endregion

    #region 파라미터 해시
    private int HashSpeed;
    private int HashAttack;
    private int HashHit;
    private int HashDeath;
    #endregion


    private void Awake()
    {
        Anim = GetComponent<Animator>();

        HashSpeed  = Animator.StringToHash("Speed");
        HashAttack = Animator.StringToHash("Attack");
        HashHit    = Animator.StringToHash("Hit");
        HashDeath  = Animator.StringToHash("Death");
    }


    // AIZombieController.Update에서 매 프레임 호출
    public void UpdateState(EZombieState state, ESpeedType speedType)
    {
        SmoothedSpeed = Mathf.MoveTowards(SmoothedSpeed, (float)speedType, Time.deltaTime / MoveDampTime);
        Anim.SetFloat(HashSpeed, SmoothedSpeed);
    }

    // 공격 트리거 — AIZombieController에서 공격 시 호출
    public void PlayAttack() => Anim.SetTrigger(HashAttack);

    // 피격 트리거 — Character_Zombie.TakeDamage에서 호출
    public void PlayHit() => Anim.SetTrigger(HashHit);

    // 사망 트리거 — Character_Zombie.OnDeath에서 호출
    public void PlayDeath() => Anim.SetTrigger(HashDeath);
}
