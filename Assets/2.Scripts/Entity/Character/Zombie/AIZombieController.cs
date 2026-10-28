using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EZombieState
{
    Idle,   // 대기 (플레이어 미감지)
    Alert,  // 경계 (소리 감지 시 해당 방향으로 이동)
    Chase,  // 추격
    Attack, // 공격
    Hit,    // 피격 (행동 중단)
    Death,  // 사망
}

// 좀비 AI 컨트롤러
// - NavMeshAgent 기반 이동 · 상태 머신 · 시각/소음 감지 · 추격 · 공격
public class AIZombieController : MonoBehaviour
{
    [Header("상태")]
    [SerializeField] private EZombieState State = EZombieState.Idle;

    [Header("시각 감지")]
    [SerializeField] private bool   UseVisualDetection = true; // true면 시각 감지 활성화
    [SerializeField] private float  ContactRange       = 10f;  // 감지 거리
    [SerializeField] private float  ContactAngle       = 120f; // 감지 각도
    [SerializeField] private FTimer Contact_Timer;             // 컨택 타이머 (Max = 컨택까지 필요한 시야 유지 시간)
    [SerializeField] private FTimer Forget_Timer;              // 망각 타이머 (Max = 시야를 잃었을 때 잊기까지 시간)

    [Header("소음 감지")]
    [SerializeField] private float NoiseResistance = 3f; // 이 값 미만의 소음 강도는 무시

    [Header("경계")]
    [SerializeField] private FTimer Alert_Timer; // 경계 타이머 (Max = 경계 상태 유지 시간)

    [Header("피격")]
    [SerializeField] private FTimer Hit_Timer; // 피격 경직 타이머 (Max = 맞은 뒤 행동하지 못하는 시간)

    [Header("공격")]
    [SerializeField] private float  AttackRange  = 1.5f; // 공격 거리
    [SerializeField] private FTimer Attack_Timer;         // 공격 쿨다운 타이머

    // 런타임 상태
    [Header("탐지된 타겟")]
    [field: SerializeField] public Character_Player Target { get; private set; } // 시각으로 감지한 플레이어 (null = 미감지)
    private Vector3          AlertPoint;       // 경계 상태에서 확인하러 갈 위치 (소음 발생 위치 · 마지막으로 본 플레이어 위치)
    private Vector3          LastSeenPosition; // 마지막으로 본 플레이어 위치
    private ESpeedType       SpeedType;    // 현재 이동 속도 타입 (ChangeState에서 갱신)
    private bool             IsHit;        // 피격 이벤트 발생 여부
    private bool             IsAttacking;  // 공격 애니메이션 진행 중 여부


    // 컴포넌트
    private Character_Zombie Zombie;
    private NavMeshAgent     Agent;



    private void Awake()
    {
        Zombie = GetComponent<Character_Zombie>();
        Agent  = GetComponent<NavMeshAgent>();
        Agent.stoppingDistance = AttackRange;

        Alert_Timer.Current = Alert_Timer.Max; // 시작 시 경계 타이머를 만료 상태로 초기화
    }

    private void Update()
    {
        if (Mgr_Game.Inst.GameState != EGameState.Play) return; // 로딩 중 — 대기

        // 시각 감지
        UpdateVisualDetection();
        
        // 상태 설정
        UpdateState();
        
        // 상태별 행동 함수 호출
        switch (State)
        {
            case EZombieState.Idle:   UpdateIdle();   break;
            case EZombieState.Alert:  UpdateAlert();  break;
            case EZombieState.Chase:  UpdateChase();  break;
            case EZombieState.Attack: UpdateAttack(); break;
            case EZombieState.Hit:    UpdateHit();    break;
        }
        
        // 매 프레임 현재 상태를 애니메이터에 반영 (SpeedType은 ChangeState에서 갱신)
        Zombie.ZombieAnim.UpdateState(State, SpeedType);
    }


    // 감지 결과에 따라 상태 전환
    private void UpdateState()
    {
        // 체력이 0 이하면 사망 상태로 전환
        if (Zombie.HP.IsEmpty)
            ChangeState(EZombieState.Death);
        // 피격 이벤트 발생 시 Hit 상태로 전환
        else if (IsHit)
        {
            ChangeState(EZombieState.Hit);
        }
        // 공격 중이거나 플레이어가 공격 범위 안에 있으면 Attack 상태 유지
        else if (IsAttacking || (Target != null && Vector3.Distance(transform.position, Target.transform.position) <= AttackRange))
            ChangeState(EZombieState.Attack);

        // Target이 있으면 추격
        else if (Target != null)
            ChangeState(EZombieState.Chase);

        // Target이 없고 Alert 타이머가 남아있으면 경계
        else if (Target == null && !Alert_Timer.IsReached)
            ChangeState(EZombieState.Alert);

        // 그 외에는 대기
        else
            ChangeState(EZombieState.Idle);
    }


    private void ChangeState(EZombieState newState)
    {
        if (State == newState) return;
        State = newState;
        switch (newState)
        {
            case EZombieState.Idle:
                SetSpeedType(ESpeedType.Idle); // 정지
                break;

            case EZombieState.Alert:
                SetSpeedType(ESpeedType.Walk); // 걷기
                Alert_Timer.Reset();           // 경계 타이머 초기화
                break;

            case EZombieState.Chase:
                SetSpeedType(ESpeedType.Walk); // 걷기
                Alert_Timer.Reset();           // 경계 타이머 초기화
                break;

            case EZombieState.Attack:
                SetSpeedType(ESpeedType.Idle);           // 공격 중 정지
                Attack_Timer.Current = Attack_Timer.Max; // 진입 즉시 첫 공격
                break;

            case EZombieState.Hit:
                SetSpeedType(ESpeedType.Idle); // 피격 중 정지
                break;

            case EZombieState.Death:
                SetSpeedType(ESpeedType.Idle); // 사망 시 정지
                Agent.enabled = false;         // NavMeshAgent 비활성화
                break;
        }
    }


    // SpeedType 변경 + Agent 속도 동기화
    private void SetSpeedType(ESpeedType speedType)
    {
        SpeedType = speedType;
        Agent.speed = speedType switch
        {
            ESpeedType.Walk => Zombie.WalkSpeed,
            _ => 0f,
        };

        // Idle이면 이동 중단
        if (speedType == ESpeedType.Idle)
        {
            Agent.ResetPath();
            Agent.velocity = Vector3.zero;
        }
    }


    #region 시각 감지
    // 플레이어가 시야에 들어오면 Target에 저장, 잊으면 null
    private void UpdateVisualDetection()
    {
        if (!UseVisualDetection)        return;
        if (State == EZombieState.Death) return;

        Character_Player player = GetPlayerInSight();
        if (player != null)
        {
            // 시야 안 — 컨택 타이머 누적, 망각 타이머 리셋
            Contact_Timer.Tick();
            Forget_Timer.Reset();

            // 컨택 타이머 완료 시 플레이어 저장
            if (Contact_Timer.IsReached)
            {
                Target           = player;
                LastSeenPosition = player.transform.position; // 보이는 동안 위치 기록
            }
        }
        else if (Contact_Timer.IsReached)
        {
            // 컨택 상태에서 시야를 잃음 — 망각 타이머 누적
            Forget_Timer.Tick();
            if (Forget_Timer.IsReached)
            {
                // 망각 시간 초과 → 플레이어를 잊음
                Target = null;
                AlertPoint = LastSeenPosition; // 마지막으로 본 위치를 확인하러 감 (경계 상태)
                Contact_Timer.Reset();
                Forget_Timer.Reset();
            }
        }
        else
        {
            // 컨택 전 시야를 잃으면 즉시 리셋
            Contact_Timer.Reset();
        }
    }

    // 시야 내 플레이어를 반환, 없으면 null
    private Character_Player GetPlayerInSight()
    {
        Vector3 toPlayer = Character_Player.Inst.transform.position - transform.position;

        // 거리 체크
        if (toPlayer.magnitude > ContactRange) return null;

        // 각도 체크 — 정면과 플레이어 방향 사이의 각도
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > ContactAngle / 2f) return null;

        // 레이캐스트 — 좀비 머리에서 플레이어 머리로
        Vector3 rayOrigin = Zombie.MeshComp.HeadBone.position;
        Vector3 rayTarget = Character_Player.Inst.MeshComp.HeadBone.position;
        Vector3 rayDir    = (rayTarget - rayOrigin).normalized;
        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, ContactRange))
        {
            Character_Player player = hit.collider.GetComponentInParent<Character_Player>();
            if (player == null)
            {
                Debug.DrawRay(rayOrigin, rayDir * hit.distance, Color.red);
                return null; // 장애물에 막힘
            }

            Debug.DrawRay(rayOrigin, rayTarget - rayOrigin, Color.green);
            return player; // 레이캐스트로 감지된 플레이어 반환
        }

        return null;
    }

    #endregion


    #region 소음 감지
    // Mgr_Noise에서 호출
    public void OnNoiseDetected(Vector3 position, float intensity)
    {
        // Idle / Alert 상태가 아니면 소음 무시
        if (State != EZombieState.Idle && State != EZombieState.Alert) return;

        // 내성 미만의 소음은 무시
        if (intensity < NoiseResistance) return;

        AlertPoint = position;
        Alert_Timer.Reset(); // 경계 타이머 시작
    }
    #endregion


    #region 대기
    // 대기 상태
    private void UpdateIdle()
    {

    }
    #endregion


    #region 경계
    // 경계 상태 — AlertPoint로 이동, 도착 후 타이머 누적
    private void UpdateAlert()
    {
        Agent.SetDestination(AlertPoint);

        // 목적지에 도착했을 때만 타이머 누적
        if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
            Alert_Timer.Tick();
    }
    #endregion


    #region 추격
    // 추격 상태 — Target 방향으로 이동
    private void UpdateChase()
    {
        if (Target == null) return;
        Agent.SetDestination(Target.transform.position);
    }
    #endregion


    #region 피격
    // 피격 상태 — 경직 시간이 지나면 해제 (OnHit에서 Hit_Timer 초기화)
    private void UpdateHit()
    {
        Hit_Timer.Tick();
        if (Hit_Timer.IsReached) IsHit = false;
    }

    // Character_Zombie.OnHit에서 호출
    public void OnHit(GameObject attacker)
    {
        // Target이 없을 때만 attacker를 Target으로 설정
        if (Target == null)
            Target = attacker.GetComponentInParent<Character_Player>();

        IsHit = true;
        Hit_Timer.Reset(); // 맞을 때마다 경직 시간을 처음부터

        if (IsAttacking) IsAttacking = false;

    }

    #endregion


    #region 공격
    // 공격 상태 — 쿨다운마다 공격
    private void UpdateAttack()
    {
        if (IsAttacking) return; // 공격 애니메이션 진행 중이면 대기

        Attack_Timer.Tick(); // 쿨다운 시간 누적

        if (Attack_Timer.IsReached) // 쿨다운이 끝나면 공격 시작
        {
            Debug.Log("좀비가 공격");

            // 공격 조건 — 하나라도 맞지 않으면 공격하지 않음 (쿨다운은 찬 상태로 유지)
            if (Target == null)    return; // 공격 대상 없음
            if (Target.HP.IsEmpty) return; // 플레이어 사망
            if (Vector3.Distance(transform.position, Target.transform.position) > AttackRange) return; // 사거리 밖

            Zombie.LookAt(Target.transform.position); // 플레이어 방향으로 즉시 회전
            if (Target.Movement.IsGrabbed) // 이미 다른 좀비에게 잡혀 있으면 합류 — 플레이어 애니메이션은 다시 시작하지 않음
                Target.JoinGrab(Zombie);   // 플레이어가 밀칠 때 같이 밀려나도록 등록
            else
                Target.OnGrabbed(Zombie);  // 플레이어에게 잡힘 신호 — 좀비와 같은 프레임에 애니메이션 시작


            Zombie.ZombieAnim.PlayAttack(); // 잡기 → 물기 → (밀침 신호 대기) → 밀려남 애니메이션 재생

            IsAttacking = true;             // 공격 중 — EndAttack 이벤트 또는 피격 시 해제
            Attack_Timer.Reset();           // 쿨다운 초기화 — 공격이 끝난 뒤부터 다시 누적
        }
    }

    // 공격 종료 — Character_Zombie.EndAttack에서 호출 (애니메이션 이벤트)
    public void OnEndAttack()
    {
        IsAttacking = false;
        Agent.Warp(transform.position); // NavMeshAgent 위치를 Root Motion이 이동한 위치로 동기화
    }    
    #endregion


    #region 사망
    // Character_Zombie.OnDeath에서 호출
    public void OnDeath()
    {
        ChangeState(EZombieState.Death);
    }
    #endregion






#if UNITY_EDITOR
    // 씬 뷰에서 정면 기준 부채꼴 감지 영역 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float   halfAngle = ContactAngle / 2f;
        Vector3 forward   = transform.forward;
        Vector3 leftDir   = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir  = Quaternion.Euler(0,  halfAngle, 0) * forward;

        // 반투명 색으로 부채꼴 채우기
        Handles.color = new Color(1f, 0f, 0f, 0.15f);
        Handles.DrawSolidArc(transform.position, Vector3.up, leftDir, ContactAngle, ContactRange);

        // 공격 사거리 원 (채우기 + 외곽선)
        Handles.color = new Color(1f, 0.3f, 0f, 0.15f);
        Handles.DrawSolidDisc(transform.position, Vector3.up, AttackRange);
        Handles.color = new Color(1f, 0.3f, 0f, 1f);
        Handles.DrawWireDisc(transform.position, Vector3.up, AttackRange);

        // 외곽선 (불투명)
        Gizmos.DrawLine(transform.position, transform.position + leftDir  * ContactRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * ContactRange);

        // 호(arc) — 세그먼트로 분할해서 그림
        int     segments  = 30;
        float   angleStep = ContactAngle / segments;
        Vector3 prevPoint = transform.position + leftDir * ContactRange;
        for (int i = 1; i <= segments; i++)
        {
            Vector3 dir       = Quaternion.Euler(0, -halfAngle + angleStep * i, 0) * forward;
            Vector3 nextPoint = transform.position + dir * ContactRange;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
#endif
}
