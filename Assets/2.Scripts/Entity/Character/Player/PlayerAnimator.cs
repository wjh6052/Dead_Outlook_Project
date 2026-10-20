using UnityEngine;

public enum EAnimatorLayer
{
    Base  = 0, // 이동, 대기
    Upper = 1, // 무기 꺼내기, 조준 상체
    Full  = 2, // 전신 공격, 공중, 앉기 시작
}

public class PlayerAnimator : MonoBehaviour
{
    #region 설정
    [Tooltip("MoveX/MoveZ 파라미터 보간 시간 (초) — 값이 클수록 방향 전환이 부드러워짐")]
    [SerializeField] private float MoveDampTime = 0.1f;
    [Tooltip("SpeedType 파라미터 보간 시간 (초) — 값이 클수록 걷기↔뛰기 전환이 부드러워짐")]
    [SerializeField] private float SpeedTypeDampTime = 0.2f;
    [Tooltip("StanceType 파라미터 보간 시간 (초) — 값이 클수록 자세 전환이 부드러워짐")]
    [SerializeField] private float StanceTypeDampTime = 0.15f;
    #endregion

    #region 내부 상태
    private Animator     Anim;
    private float        SmoothedSpeedType;
    private float        SmoothedStanceType;
    private float        SmoothedMoveX;
    private float        SmoothedMoveZ;
    private EStanceType  PrevStanceType;
    public  bool         IsAim { get; private set; }
    #endregion

    #region 파라미터 해시
    private int HashSpeedType;
    private int HashStanceType;
    private int HashMoveX;
    private int HashMoveZ;
    private int HashIsAim;
    private int HashIsGrounded;
    private int HashDoJump;
    private int HashOnCrouch;
    private int HashOnStand;
    private int HashTakeWeapon;
    private int HashHolsterWeapon;
    private int HashWeaponType;
    private int HashWeaponMeleeType;
    private int  HashIsFocusing;
    private int  HashFocusType;
    private int  HashOnFocusStart;
    private int  HashOnGrabbed;
    private bool PrevIsFocusing;
    #endregion

    private void Awake()
    {
        Anim           = GetComponent<Animator>();

        HashSpeedType  = Animator.StringToHash("SpeedType");
        HashStanceType = Animator.StringToHash("StanceType");
        HashMoveX      = Animator.StringToHash("MoveX");
        HashMoveZ      = Animator.StringToHash("MoveZ");
        HashIsAim      = Animator.StringToHash("IsAim");
        HashIsGrounded = Animator.StringToHash("IsGrounded");
        HashDoJump     = Animator.StringToHash("DoJump");
        HashOnCrouch   = Animator.StringToHash("OnCrouch");
        HashOnStand    = Animator.StringToHash("OnStand");
        HashTakeWeapon      = Animator.StringToHash("TakeWeapon");
        HashHolsterWeapon   = Animator.StringToHash("HolsterWeapon");
        HashWeaponType      = Animator.StringToHash("WeaponType");
        HashWeaponMeleeType = Animator.StringToHash("WeaponMeleeType");
        HashIsFocusing      = Animator.StringToHash("IsFocusing");
        HashFocusType       = Animator.StringToHash("FocusType");
        HashOnFocusStart    = Animator.StringToHash("OnFocusStart");
        HashOnGrabbed       = Animator.StringToHash("OnGrabbed");
    }

    // PlayerMovement의 AfterCharacterUpdate에서 호출 — 애니메이터 파라미터 갱신
    public void UpdateAnimator(EStanceType stanceType, ESpeedType speedType, bool isGrounded, bool isAim, bool doJump, Vector2 moveInput, bool isGrabbed)
    {
        // 이동 방향 보간
        float moveStep = Time.deltaTime / MoveDampTime;
        SmoothedMoveX = Mathf.MoveTowards(SmoothedMoveX, moveInput.x, moveStep);
        SmoothedMoveZ = Mathf.MoveTowards(SmoothedMoveZ, moveInput.y, moveStep);
        Anim.SetFloat(HashMoveX, SmoothedMoveX);
        Anim.SetFloat(HashMoveZ, SmoothedMoveZ);

        // 속도 타입 보간
        SmoothedSpeedType = Mathf.MoveTowards(SmoothedSpeedType, (float)speedType, Time.deltaTime / SpeedTypeDampTime);
        Anim.SetFloat(HashSpeedType, SmoothedSpeedType);

        // 웅크리기/일어서기 트리거 — 상태 전환 첫 프레임에만 발동
        // 잡혀 있는 동안에는 트리거 없이 자세 기록만 갱신 (잡힘 애니메이션이 끊기지 않도록)
        if (!isGrabbed)
        {
            if (PrevStanceType != EStanceType.Crouch && stanceType == EStanceType.Crouch)
                Anim.SetTrigger(HashOnCrouch);
            else if (PrevStanceType == EStanceType.Crouch && stanceType != EStanceType.Crouch)
                Anim.SetTrigger(HashOnStand);
        }
        PrevStanceType = stanceType;

        // 상태 플래그
        IsAim = isAim;
        Anim.SetBool(HashIsAim,      isAim);
        SmoothedStanceType = Mathf.MoveTowards(SmoothedStanceType, (float)stanceType, Time.deltaTime / StanceTypeDampTime);
        Anim.SetFloat(HashStanceType, SmoothedStanceType);
        Anim.SetBool(HashIsGrounded, isGrounded);

        // 점프 트리거 — doJump는 PlayerMovement에서 이미 상승 엣지 처리됨
        if (doJump) Anim.SetTrigger(HashDoJump);
    }


    public void SetWeaponType(int weaponType, int weaponMeleeType)
    {
        Anim.SetFloat(HashWeaponType,      weaponType);
        Anim.SetFloat(HashWeaponMeleeType, weaponMeleeType);
    }


    // Character_Player에서 IsFocusing, FocusType을 읽어 애니메이터 파라미터 갱신
    // false → true 전환 시 OnFocusStart 트리거 발동
    public void UpdateFocus()
    {
        bool isFocusing = Character_Player.Inst.IsFocusing;

        if (!PrevIsFocusing && isFocusing)
            Anim.SetTrigger(HashOnFocusStart);

        Anim.SetBool(HashIsFocusing,  isFocusing);
        Anim.SetFloat(HashFocusType, (float)Character_Player.Inst.FocusType);

        PrevIsFocusing = isFocusing;
    }

    public void PlayTakeWeapon()    => Anim.SetTrigger(HashTakeWeapon);
    public void PlayHolsterWeapon() => Anim.SetTrigger(HashHolsterWeapon);

    // 잡힘 → 물림 → 밀치기 재생 — 이후 단계는 애니메이터에서 Exit Time으로 연결
    public void PlayGrabbed() => Anim.SetTrigger(HashOnGrabbed);

    // 지정한 레이어를 Empty 상태로 부드럽게 전환 — 애니메이션 종료 시 사용
    public void PlayEmpty(EAnimatorLayer layer) => Anim.CrossFade("Empty", 0.2f, (int)layer);

    // 무기·콤보에 맞는 공격 애니메이션 재생 — 해당 애니메이션이 없으면 false 반환
    public bool PlayAttack(Item_Weapon weapon, int Combo)
    {
        string stateName = "";

        switch (weapon.WeaponType)
        {
            case EWeaponType.Melee:
                if (weapon is Item_Weapon_Melee melee)
                    stateName = $"{melee.WeaponMeleeType}";
                break;

            case EWeaponType.Ranged:
                break;
        }

        if (Anim.HasState((int)EAnimatorLayer.Upper, Animator.StringToHash($"{stateName}_Attack_{Combo}")))
        {
            Anim.CrossFade($"{stateName}_Attack_{Combo}", 0.1f, (int)EAnimatorLayer.Upper);
            return true;
        }

        Debug.Log($"[{stateName}_Attack_{Combo}] 애니메이션이 없음");
        return false;
    }
}
