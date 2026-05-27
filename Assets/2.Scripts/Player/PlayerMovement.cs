using UnityEngine;
using KinematicCharacterController;

public class PlayerMovement : MonoBehaviour, ICharacterController, IControllable
{
    private KinematicCharacterMotor KCCMotor;
    private Entity_Player EntityPlayer;
    private Vector2 MoveInput;

    [SerializeField] private float RotationSpeed = 10f;

    private void Awake()
    {
        KCCMotor = GetComponent<KinematicCharacterMotor>();
        KCCMotor.CharacterController = this;
        EntityPlayer = GetComponent<Entity_Player>();
    }

    #region IControllable
    // Mgr_PlayerController로부터 매 프레임 WASD 입력값을 받아 저장
    public void SetMoveInput(Vector2 input)
    {
        MoveInput = input;
    }
    #endregion

    
    // MoveInput(Vector2)을 카메라 기준 3D 이동 방향(Vector3)으로 변환
    private Vector3 GetMoveDirection()
    {
        if (MoveInput.sqrMagnitude < 0.01f) return Vector3.zero;

        Transform cam = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;

        return (forward * MoveInput.y + right * MoveInput.x).normalized;
    }


    #region 매 프레임 KCC가 호출
    // 캐릭터 속도 결정 (지면이면 이동속도 적용, 공중이면 중력 누적)
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (KCCMotor.GroundingStatus.IsStableOnGround)
        {
            currentVelocity = GetMoveDirection() * EntityPlayer.GetMoveSpeed();
        }
        else
        {
            currentVelocity += Physics.gravity * deltaTime;
        }
    }


    // 캐릭터 회전 결정
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        Vector3 moveDir = GetMoveDirection();
        if (moveDir.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        currentRotation = Quaternion.Slerp(currentRotation, targetRotation, RotationSpeed * deltaTime);
    }


    // KCC 업데이트 전 호출 - 입력값 읽기, 상태 초기화 등
    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    // 착지 판정 후 호출 - 점프/중력 상태 처리
    public void PostGroundingUpdate(float deltaTime)
    {

    }

    // KCC 업데이트 후 호출 - 쪼그리기 높이 변경 등 후처리
    public void AfterCharacterUpdate(float deltaTime)
    {

    }
    #endregion

    #region KCC 콜백 (당장 불필요)
    // 특정 콜라이더와 충돌할지 여부 결정 - false 반환 시 통과 (ex. 아군 관통)
    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    // 바닥 착지 시 호출 (ex. 착지 사운드, 이펙트)
    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    // 이동 중 벽/장애물 충돌 시 호출 (ex. 충돌 사운드, 속도 감소)
    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) { }

    // 충돌 안정성 커스터마이징 - 경사면을 바닥으로 인정할지 여부 등
    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

    // 이동 외 물리 충돌 감지 시 호출 (ex. 날아오는 물체에 맞았을 때)
    public void OnDiscreteCollisionDetected(Collider hitCollider) { }
    #endregion
}
