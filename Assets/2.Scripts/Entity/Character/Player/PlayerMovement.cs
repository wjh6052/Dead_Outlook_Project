using UnityEngine;
using KinematicCharacterController;

// 이동 속도 상태
public enum ESpeedType
{
    Idle,  // 정지
    Walk,  // 걷기
    Run,   // 뛰기
}

// 자세 상태
public enum EStanceType
{
    Stand,  // 서있음
    Crouch, // 웅크림
}

public class PlayerMovement : MonoBehaviour, ICharacterController
{
    #region 입력 (Mgr_PlayerController에서 매 프레임 설정)
    public Vector2 MoveInput    { get; set; } // 이동 입력 (WASD)
    public bool    bRunInput    { get; set; } // 달리기 버튼
    public bool    bJump        { get; set; } // 점프 버튼
    public bool    bAim         { get; set; } // 조준 버튼
    public bool    bCrouchInput { get; set; } // 웅크리기 버튼

    #endregion

    #region 상태
    [field: Tooltip("현재 이동 속도 (m/s)")]
    [field: SerializeField] public float CurrentSpeed { get; private set; } // 현재 이동 속도 (m/s)
    [field: Tooltip("자세 상태 (Stand / Crouch)")]
    [field: SerializeField] public EStanceType StanceType   { get; private set; } // 자세 상태 (Stand / Crouch / Air)
    [field: Tooltip("이동 속도 상태 (Idle / Walk / Run)")]
    [field: SerializeField] public ESpeedType  SpeedType    { get; private set; } // 이동 속도 상태 (Idle / Walk / Run)
    [field: Tooltip("지면 접촉 여부")]
    [field: SerializeField] public bool        bIsGrounded  { get; private set; } // 지면 접촉 여부
    [field: Tooltip("좀비에게 잡혀 있는지 여부")]
    [field: SerializeField] public bool        IsGrabbed    { get; private set; } // 좀비에게 잡혀 있는지 여부

    #endregion


    [Header("이동")]
    [Tooltip("걷기 이동 속도 (m/s)")]
    [SerializeField] private float WalkSpeed  = 3f; // 걷기 이동 속도 (m/s)
    [Tooltip("달리기 이동 속도 (m/s)")]
    [SerializeField] private float RunSpeed   = 5f; // 달리기 이동 속도 (m/s)

    [Header("웅크리기")]
    [Tooltip("웅크리며 걷는 속도 (m/s)")]
    [SerializeField] private float CrouchSpeed    = 2f; // 웅크리며 걷는 속도 (m/s)
    [Tooltip("웅크리며 뛰는 속도 (m/s)")]
    [SerializeField] private float CrouchRunSpeed = 3f; // 웅크리며 뛰는 속도 (m/s)
    [Tooltip("서 있을 때 캡슐 높이 (m)")]
    [SerializeField] private float StandHeight  = 2f; // 기립 시 캡슐 높이 (m)
    [Tooltip("웅크렸을 때 캡슐 높이 (m)")]
    [SerializeField] private float CrouchHeight = 1f; // 웅크리기 시 캡슐 높이 (m)

    [Header("점프")]
    [Tooltip("점프 시 위로 가해지는 힘")]
    [SerializeField] private float JumpForce = 5f; // 점프 시 위로 가해지는 힘

    [Header("회전")]
    [Tooltip("캐릭터 회전 속도 — 높을수록 즉각 회전")]
    [SerializeField] private float RotationSpeed = 15f; // 캐릭터 회전 속도 — 높을수록 즉각 회전

    private bool bPrevCrouch;    // 이전 프레임 웅크리기 버튼 상태 — 상승 엣지 감지용
    private bool bPrevJump;      // 이전 프레임 점프 버튼 상태 — 상승 엣지 감지용
    private bool bPrevGrounded;  // 이전 프레임 지면 접촉 여부 — 착지 감지용
    [HideInInspector] public bool doJump; // 이번 프레임 점프 실행 플래그

    private Vector3 RootMotionVelocity; // 루트 모션 속도 캐시 — OnAnimatorMove에서 갱신
    private static readonly int HashRootMotionTag = UnityEngine.Animator.StringToHash("RootMotion"); // 루트 모션으로 이동할 애니메이터 상태 태그

    private Character_Player        Player;   // 플레이어 엔티티
    private KinematicCharacterMotor KCC;      // KCC 물리 처리 컴포넌트
    private PlayerAnimator          Animator; // 애니메이터 — AfterCharacterUpdate에서 값 전달
    private Animator                Anim;     // Unity Animator — deltaPosition 접근용

    private void Awake()
    {
        Player   = GetComponent<Character_Player>();
        KCC      = GetComponent<KinematicCharacterMotor>();
        Animator = GetComponent<PlayerAnimator>();
        Anim     = GetComponentInChildren<Animator>(); // Animator는 자식 모델에 있음
        KCC.CharacterController = this; // KCC가 이 스크립트를 컨트롤러로 사용하도록 등록
    }

    // Apply Root Motion 대신 Unity가 호출 — 루트 모션 델타를 KCC 속도로 변환해 캐싱
    // 모델의 PlayerAnimationEvents.OnAnimatorMove에서 호출
    public void UpdateRootMotion()
    {
        RootMotionVelocity = Anim.deltaPosition / Time.deltaTime;
    }

    // 웅크리기 토글 — 조건 확인 후 SetCrouch 호출
    private void TryToggleCrouch()
    {
        if (StanceType != EStanceType.Crouch)
            SetCrouch(true);
        else if (CanStandUp())
            SetCrouch(false);
    }

    // 웅크리기 상태 설정 — 캡슐 크기·StanceType·카메라 높이 일괄 처리
    private void SetCrouch(bool isCrouching)
    {
        if (isCrouching)
        {
            StanceType = EStanceType.Crouch;
            KCC.SetCapsuleDimensions(KCC.Capsule.radius, CrouchHeight, CrouchHeight * 0.5f);
        }
        else
        {
            StanceType = EStanceType.Stand;
            KCC.SetCapsuleDimensions(KCC.Capsule.radius, StandHeight, StandHeight * 0.5f);
        }
        Player.SetCrouching(isCrouching);
    }

    // 머리 위 공간 확인 — 캡슐 캐스트로 일어설 공간이 있는지 검사
    private bool CanStandUp()
    {
        float   r      = KCC.Capsule.radius;
        Vector3 bottom = KCC.TransientPosition + Vector3.up * r;           // 캡슐 하단 구체 중심
        Vector3 top    = KCC.TransientPosition + Vector3.up * (StandHeight - r); // 캡슐 상단 구체 중심
        int     mask   = 1 << 0;                                              // Default 레이어만 체크
        return !Physics.CheckCapsule(bottom, top, r - 0.01f, mask, QueryTriggerInteraction.Ignore);
    }

    // 카메라 방향 기준으로 이동 입력을 월드 방향으로 변환
    public Vector3 GetDesiredMoveDirection()
    {
        if (MoveInput.sqrMagnitude < 0.01f) return Vector3.zero; // 입력 없으면 zero 반환

        // 카메라의 forward/right를 수평면에 투영해 Y축 회전만 반영
        Transform cam     = Camera.main.transform;
        Vector3   forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3   right   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;

        return (forward * MoveInput.y + right * MoveInput.x).normalized;
    }

    // 대상 위치를 향해 즉시 회전 — 높이 차이는 무시하고 수평으로만 회전
    public void LookAt(Vector3 targetPosition)
    {
        Vector3 dir = Vector3.ProjectOnPlane(targetPosition - KCC.TransientPosition, Vector3.up);
        if (dir.sqrMagnitude < 0.0001f) return; // 같은 위치면 방향을 정할 수 없음

        KCC.SetRotation(Quaternion.LookRotation(dir)); // KCC가 회전을 관리하므로 transform 대신 KCC로 지정
    }

    // 지정한 위치 · 방향으로 즉시 이동 — KCC가 위치를 관리하므로 transform 대신 KCC로 지정 (맵 이동 스폰 등)
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        KCC.SetPositionAndRotation(position, rotation);
    }

    // 잡힘 — Character_Player.OnGrabbed에서 호출
    // grabberPosition : 잡은 좀비의 위치 (회전 방향 계산용)
    public void OnGrabbed(Vector3 grabberPosition)
    {
        IsGrabbed = true;
        SetCrouch(false);         // 웅크리기 — 선 자세로 초기화
        LookAt(grabberPosition);  // 좀비 방향으로 즉시 회전
    }

    // 잡힘 종료 — Character_Player.OnGrabEnd에서 호출
    public void OnGrabEnd()
    {
        IsGrabbed = false;
    }

    // 현재 재생 중인 상태에 RootMotion 태그가 있는지 확인 — 전환 중이면 다음 상태도 확인
    private bool IsRootMotionState()
    {
        for (int layer = 0; layer < Anim.layerCount; layer++)
        {
            if (Anim.GetCurrentAnimatorStateInfo(layer).tagHash == HashRootMotionTag) return true;
            if (Anim.IsInTransition(layer) && Anim.GetNextAnimatorStateInfo(layer).tagHash == HashRootMotionTag) return true;
        }
        return false;
    }


    #region KCC — 매 프레임 호출
    // KCC 업데이트 전 호출 — 입력 전처리, 상태 변경 등 선행 작업
    public void BeforeCharacterUpdate(float deltaTime)
    {
        // 웅크리기 — 버튼을 누른 첫 프레임(상승 엣지)에만 토글
        if (bCrouchInput && !bPrevCrouch) TryToggleCrouch();
        bPrevCrouch = bCrouchInput;

        // 점프 — 지면에 있고 버튼을 누른 첫 프레임에만 실행
        doJump    = bJump && bIsGrounded && !bPrevJump;
        bPrevJump = bJump;

        // 이동 속도 상태 결정
        if      (MoveInput.sqrMagnitude < 0.01f) SpeedType = ESpeedType.Idle;
        else if (bRunInput)                       SpeedType = ESpeedType.Run;
        else                                      SpeedType = ESpeedType.Walk;
    }

    // 캐릭터 속도 결정 — 이동·점프·중력 등 currentVelocity 값을 직접 설정
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (KCC.GroundingStatus.IsStableOnGround)
        {
            // RootMotion 태그 상태면 루트 모션 속도 사용 — 애니메이션의 XZ 이동을 KCC에 전달
            if (IsRootMotionState())
            {
                currentVelocity = RootMotionVelocity;
                return;
            }

            // 자세 + 속도 상태 조합으로 현재 속도 결정
            CurrentSpeed = (StanceType, SpeedType) switch
            {
                (EStanceType.Crouch, ESpeedType.Run)  => CrouchRunSpeed,
                (EStanceType.Crouch, ESpeedType.Walk) => CrouchSpeed,
                (_,                  ESpeedType.Run)  => RunSpeed,
                (_,                  ESpeedType.Walk) => WalkSpeed,
                _                                     => 0f,
            };
            currentVelocity = GetDesiredMoveDirection() * CurrentSpeed;

            if (doJump)
            {
                if (StanceType == EStanceType.Crouch) TryToggleCrouch(); // 웅크린 상태면 일어서기만
                else
                {
                    KCC.ForceUnground();                       // KCC의 지면 고정 해제
                    currentVelocity += Vector3.up * JumpForce; // 위로 힘 적용
                }
            }
        }
        else
        {
            // 공중 — 중력 누적
            currentVelocity += Physics.gravity * deltaTime;
        }
    }

    // 캐릭터 회전 결정 — currentRotation 값을 직접 설정
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // 조준 중이면 카메라 방향, 아니면 이동 방향으로 회전
        Vector3 lookDir = bAim ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up)
                               : GetDesiredMoveDirection();
        if (lookDir.sqrMagnitude < 0.01f) return; // 방향이 없으면 회전 안 함
        currentRotation = Quaternion.Slerp(currentRotation, Quaternion.LookRotation(lookDir), RotationSpeed * deltaTime);
    }

    // 착지 판정 직후 호출 — 착지 이벤트 처리 (사운드, 이펙트 등)
    public void PostGroundingUpdate(float deltaTime) { }

    // KCC 업데이트 후 호출 — 상태 갱신, 디버그 값 반영 등 후처리
    public void AfterCharacterUpdate(float deltaTime)
    {
        bIsGrounded = KCC.GroundingStatus.IsStableOnGround;

        if (!bIsGrounded)
        {
            if (StanceType == EStanceType.Crouch) SetCrouch(false); // 웅크린 채로 공중 진입 시 즉시 일어서기
        }
        else if (!bPrevGrounded) SetCrouch(false); // 착지 순간 Stand로 복귀
        bPrevGrounded = bIsGrounded;

        Animator.UpdateAnimator(StanceType, SpeedType, bIsGrounded, bAim, doJump, MoveInput, IsGrabbed);
    }
    #endregion


    #region KCC — 충돌 콜백
    // 특정 콜라이더와 충돌할지 여부 — false 반환 시 통과 (예: 아군 관통)
    public bool IsColliderValidForCollisions(Collider coll) { return true; }

    // 바닥 착지 시 호출 — 착지 사운드, 이펙트 등
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    // 이동 중 벽·장애물 충돌 시 호출 — 충돌 사운드, 속도 감소 등
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    // 충돌 안정성 커스터마이징 — 경사면을 바닥으로 인정할지 등
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    // 이동 외 물리 충돌 감지 시 호출 — 날아오는 물체에 맞았을 때 등
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    #endregion
}
