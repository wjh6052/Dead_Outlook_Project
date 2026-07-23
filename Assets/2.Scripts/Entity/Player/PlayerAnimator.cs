using UnityEngine;

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
    #endregion

    private void Awake()
    {
        Anim           = GetComponentInChildren<Animator>();

        HashSpeedType  = Animator.StringToHash("SpeedType");
        HashStanceType = Animator.StringToHash("StanceType");
        HashMoveX      = Animator.StringToHash("MoveX");
        HashMoveZ      = Animator.StringToHash("MoveZ");
        HashIsAim      = Animator.StringToHash("IsAim");
        HashIsGrounded = Animator.StringToHash("IsGrounded");
        HashDoJump     = Animator.StringToHash("DoJump");
        HashOnCrouch   = Animator.StringToHash("OnCrouch");
        HashOnStand    = Animator.StringToHash("OnStand");
    }

    // PlayerMovement의 AfterCharacterUpdate에서 호출 — 애니메이터 파라미터 갱신
    public void UpdateAnimator(EStanceType stanceType, ESpeedType speedType, bool isGrounded, bool isAim, bool doJump, Vector2 moveInput)
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
        if (PrevStanceType != EStanceType.Crouch && stanceType == EStanceType.Crouch)
            Anim.SetTrigger(HashOnCrouch);
        else if (PrevStanceType == EStanceType.Crouch && stanceType != EStanceType.Crouch)
            Anim.SetTrigger(HashOnStand);
        PrevStanceType = stanceType;

        // 상태 플래그
        Anim.SetBool(HashIsAim,      isAim);
        SmoothedStanceType = Mathf.MoveTowards(SmoothedStanceType, (float)stanceType, Time.deltaTime / StanceTypeDampTime);
        Anim.SetFloat(HashStanceType, SmoothedStanceType);
        Anim.SetBool(HashIsGrounded, isGrounded);

        // 점프 트리거 — doJump는 PlayerMovement에서 이미 상승 엣지 처리됨
        if (doJump) Anim.SetTrigger(HashDoJump);
    }
}
