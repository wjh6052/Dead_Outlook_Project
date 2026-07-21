using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Tooltip("MoveX/MoveZ 파라미터 보간 시간 (초) — 값이 클수록 방향 전환이 부드러워짐")]
    [SerializeField] private float MoveDampTime = 0.1f;
    [Tooltip("SpeedType 파라미터 보간 시간 (초) — 값이 클수록 걷기↔뛰기 전환이 부드러워짐")]
    [SerializeField] private float SpeedTypeDampTime = 0.2f;

    private Animator       Anim;
    private PlayerMovement Movement;
    private float          SmoothedSpeedType;
    private bool           bPrevJump; // Jump 트리거 상승 엣지 감지용
    private float          SmoothedMoveX;
    private float          SmoothedMoveZ;

    private static readonly int HashSpeed     = Animator.StringToHash("Speed");
    private static readonly int HashSpeedType = Animator.StringToHash("SpeedType");
    private static readonly int HashMoveX     = Animator.StringToHash("MoveX");
    private static readonly int HashMoveZ     = Animator.StringToHash("MoveZ");
    private static readonly int HashIsAim      = Animator.StringToHash("IsAim");
    private static readonly int HashIsCrouch  = Animator.StringToHash("IsCrouch");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump       = Animator.StringToHash("Jump");


    private void Awake()
    {
        Anim     = GetComponent<Animator>();
        Movement = GetComponentInParent<PlayerMovement>();
    }

    private void Update()
    {
        Anim.SetFloat(HashSpeed, Movement.GetNormalizedSpeed());

        SmoothedSpeedType = Mathf.MoveTowards(SmoothedSpeedType, (float)Movement.SpeedType, Time.deltaTime / SpeedTypeDampTime);
        Anim.SetFloat(HashSpeedType, SmoothedSpeedType);

        float moveSpeed = Time.deltaTime / MoveDampTime;
        SmoothedMoveX = Mathf.MoveTowards(SmoothedMoveX, Movement.MoveInput.x, moveSpeed);
        SmoothedMoveZ = Mathf.MoveTowards(SmoothedMoveZ, Movement.MoveInput.y, moveSpeed);
        Anim.SetFloat(HashMoveX, SmoothedMoveX);
        Anim.SetFloat(HashMoveZ, SmoothedMoveZ);

        Anim.SetBool(HashIsAim,      Movement.bAim);
        Anim.SetBool(HashIsCrouch,   Movement.bIsCrouching);
        Anim.SetBool(HashIsGrounded, Movement.bIsGrounded);

        if (Movement.bJump && !bPrevJump && !Movement.bIsCrouching)
            Anim.SetTrigger(HashJump);
        bPrevJump = Movement.bJump;
    }
}
