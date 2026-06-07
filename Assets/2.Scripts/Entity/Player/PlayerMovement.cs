using UnityEngine;
using KinematicCharacterController;

public enum ESpeedType { Walk, Run }

public class PlayerMovement : MonoBehaviour, ICharacterController
{
    #region 입력
    public Vector2    MoveInput    { get; set; } // 이동 입력 (WASD)
    public bool       bAim         { get; set; } // 조준 중 여부
    public bool       bCrouchInput { get; set; } // 웅크리기 버튼 상태

    private bool _bJump;
    private bool _bJumpRaw; // 상승 엣지 감지를 위한 raw 입력값
    public bool bJump
    {
        get => _bJump;
        set { _bJumpRaw = value; _bJump = value && bIsGrounded; } // 지면에 있을 때만 true 허용
    }
    #endregion


    #region 상태
    public ESpeedType SpeedType    { get; set; }          // 현재 이동 속도 타입 (Walk/Run)
    public bool       bIsGrounded  { get; private set; }  // 지면 접촉 여부
    public bool       bIsCrouching { get; private set; }  // 웅크린 상태 여부
    #endregion


    // 상승 엣지 감지 및 이동 내부 상태
    private bool    bPrevCrouchInput; // 이전 프레임 웅크리기 버튼 상태 — 상승 엣지 감지용
    private bool    bPrevJumpInput;   // 이전 프레임 점프 버튼 상태 — 상승 엣지 감지용
    private Vector3 HorizontalVelocity;  // 수평 속도 벡터 — 관성 계산에 사용
    private float   CurrentSpeed;        // 현재 실제 이동 속도
    private float   CrouchTransitionTimer; // 웅크리기 전환 중 속도 감소 타이머

    [Tooltip("웅크리기 전환 시 이동 불가 시간 (초)")]
    [SerializeField] private float CrouchTransitionDuration = 0.5f;

    
    #region 디버그 (읽기 전용)
    [Header("디버그 (읽기 전용)")]
    [Tooltip("이동 입력 (WASD)")]
    [SerializeField] private Vector2 Debug_MoveInput;
    [Tooltip("점프 버튼 상태")]
    [SerializeField] private bool Debug_Jump;
    [Tooltip("현재 이동 속도 타입")]
    [SerializeField] private ESpeedType Debug_SpeedType;
    [Tooltip("현재 실제 이동 속도")]
    [SerializeField] private float Debug_CurrentSpeed;
    [Tooltip("지면 접촉 여부")]
    [SerializeField] private bool Debug_IsGrounded;
    [Tooltip("조준 중 여부")]
    [SerializeField] private bool Debug_IsAim;
    [Tooltip("웅크리기 버튼 상태")]
    [SerializeField] private bool Debug_IsCrouching;
    #endregion


    private KinematicCharacterMotor KCCMotor;    // KCC 물리 처리 컴포넌트
    private Entity_Player           EntityPlayer; // 플레이어 스탯 참조

    private void Awake()
    {
        KCCMotor = GetComponent<KinematicCharacterMotor>();
        KCCMotor.CharacterController = this;
        EntityPlayer = GetComponent<Entity_Player>();
    }

    // 현재 속도를 최대 속도 기준으로 0~1로 정규화 — 애니메이터 Speed 파라미터에 사용
    public float GetNormalizedSpeed()
    {
        float maxSpeed = bIsCrouching ? EntityPlayer.GetCrouchSpeed()
            : SpeedType == ESpeedType.Run ? EntityPlayer.GetRunSpeed()
            : EntityPlayer.GetWalkSpeed();
        return Mathf.Clamp01(CurrentSpeed / maxSpeed);
    }

    // 카메라 방향 기준으로 이동 입력을 월드 방향으로 변환
    public Vector3 GetDesiredMoveDirection()
    {
        if (MoveInput.sqrMagnitude < 0.01f) return Vector3.zero;

        Transform cam     = Camera.main.transform;
        Vector3   forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3   right   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

        return (forward * MoveInput.y + right * MoveInput.x).normalized;
    }

    // 웅크리기 토글 — 앉기/일어서기 시 KCC 캡슐 높이 조정
    private void TryToggleCrouch()
    {
        if (!bIsCrouching)
        {
            bIsCrouching = true;
            float h = EntityPlayer.GetCrouchHeight();
            KCCMotor.SetCapsuleDimensions(KCCMotor.Capsule.radius, h, h * 0.5f);
            CrouchTransitionTimer = CrouchTransitionDuration;
        }
        else if (CanStandUp())
        {
            bIsCrouching = false;
            float h = EntityPlayer.GetStandHeight();
            KCCMotor.SetCapsuleDimensions(KCCMotor.Capsule.radius, h, h * 0.5f);
            CrouchTransitionTimer = CrouchTransitionDuration;
        }
    }

    // 머리 위 장애물 확인 — 일어설 공간이 있으면 true
    private bool CanStandUp()
    {
        float   r      = KCCMotor.Capsule.radius;
        float   standH = EntityPlayer.GetStandHeight();
        Vector3 bottom = KCCMotor.TransientPosition + Vector3.up * r;
        Vector3 top    = KCCMotor.TransientPosition + Vector3.up * (standH - r);
        int     mask   = ~(1 << gameObject.layer);
        return !Physics.CheckCapsule(bottom, top, r - 0.01f, mask, QueryTriggerInteraction.Ignore);
    }


    #region KCC 매 프레임 호출
    // KCC 업데이트 전 호출 — 웅크리기 토글 및 점프 상승 엣지 처리
    public void BeforeCharacterUpdate(float deltaTime)
    {
        if (bCrouchInput && !bPrevCrouchInput)
            TryToggleCrouch();
        bPrevCrouchInput = bCrouchInput;

        _bJump         = _bJump && !bPrevJumpInput; // 상승 엣지만 처리 (지면 체크는 setter에서)
        bPrevJumpInput = _bJumpRaw;

        if (CrouchTransitionTimer > 0f)
            CrouchTransitionTimer -= deltaTime;
    }

    // 캐릭터 속도 결정 (지면이면 이동속도 적용, 공중이면 중력 누적)
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (KCCMotor.GroundingStatus.IsStableOnGround)
        {
            float targetSpeed = bIsCrouching ? EntityPlayer.GetCrouchSpeed()
                : SpeedType == ESpeedType.Run ? EntityPlayer.GetRunSpeed()
                : EntityPlayer.GetWalkSpeed();

            if (CrouchTransitionTimer > 0f)
                targetSpeed = 0f;

            Vector3 targetVel = GetDesiredMoveDirection() * targetSpeed;

            float rate = targetVel.sqrMagnitude > 0.01f
                ? EntityPlayer.GetMoveAcceleration()
                : EntityPlayer.GetMoveDeceleration();

            HorizontalVelocity = Vector3.MoveTowards(HorizontalVelocity, targetVel, rate * deltaTime);
            CurrentSpeed       = HorizontalVelocity.magnitude;

            currentVelocity = new Vector3(HorizontalVelocity.x, 0f, HorizontalVelocity.z);

            if (bJump)
            {
                if (bIsCrouching)
                    TryToggleCrouch(); // 앉은 상태면 일어서기만
                else
                {
                    KCCMotor.ForceUnground();
                    currentVelocity += Vector3.up * EntityPlayer.GetJumpForce();
                }
            }
        }
        else
        {
            currentVelocity  = new Vector3(HorizontalVelocity.x, currentVelocity.y, HorizontalVelocity.z);
            currentVelocity += Physics.gravity * deltaTime;
        }
    }

    // 캐릭터 회전 결정
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (bAim)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
            if (camForward.sqrMagnitude < 0.01f) return;
            currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(camForward), EntityPlayer.GetRotationSpeed() * deltaTime);
        }
        else
        {
            Vector3 moveDir = GetDesiredMoveDirection();
            if (moveDir.sqrMagnitude < 0.01f) return;
            currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(moveDir), EntityPlayer.GetRotationSpeed() * deltaTime);
        }
    }

    // 착지 판정 후 호출 — 점프/중력 상태 처리
    public void PostGroundingUpdate(float deltaTime) { }

    // KCC 업데이트 후 호출 — 후처리
    public void AfterCharacterUpdate(float deltaTime)
    {
        bIsGrounded = KCCMotor.GroundingStatus.IsStableOnGround;

        Debug_MoveInput = MoveInput;
        Debug_Jump = bJump;
        Debug_SpeedType = SpeedType;
        Debug_CurrentSpeed = CurrentSpeed;
        Debug_IsGrounded = bIsGrounded;
        Debug_IsAim = bAim;
        Debug_IsCrouching = bCrouchInput;
    }
    #endregion


    #region KCC 콜백 (당장 불필요)
    // 특정 콜라이더와 충돌할지 여부 결정 — false 반환 시 통과 (ex. 아군 관통)
    public bool IsColliderValidForCollisions(Collider coll) { return true; }
    // 바닥 착지 시 호출 (ex. 착지 사운드, 이펙트)
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    // 이동 중 벽/장애물 충돌 시 호출 (ex. 충돌 사운드, 속도 감소)
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }
    // 충돌 안정성 커스터마이징 — 경사면을 바닥으로 인정할지 여부 등
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }
    // 이동 외 물리 충돌 감지 시 호출 (ex. 날아오는 물체에 맞았을 때)
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    #endregion
}
