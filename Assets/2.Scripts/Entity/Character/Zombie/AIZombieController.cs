using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum EZombieState
{
    Idle,   // 대기 (플레이어 미감지)
    Alert,  // 경계 (소리 감지 시 해당 방향으로 이동 — 소음 시스템 구현 후 사용)
    Chase,  // 추격
    Attack, // 공격
}

// 좀비 AI 컨트롤러
// - NavMeshAgent 기반 이동 · 상태 머신 · 감지/추격/공격
public class AIZombieController : MonoBehaviour
{
    [Header("테스트")]
    [SerializeField] private GameObject ContactObject;        // 컨택 테스트용 오브젝트
    [SerializeField] private bool       bTest;                // true면 플레이어 감지 없이 Idle 상태 고정

    [Header("감지")]
    [SerializeField] private float   ContactRange  = 10f;  // 감지 거리
    [SerializeField] private float   ContactAngle  = 120f; // 감지 각도
    [SerializeField] private FTimer  Contact_Timer;        // 컨택 타이머 (Max = 컨택까지 필요한 시야 유지 시간)

    [Header("망각")]
    [SerializeField] private FTimer  Forget_Timer;         // 망각 타이머 (Max = 컨택 후 시야를 잃었을 때 잊기까지 시간)

    [Header("경계")]
    [SerializeField] private FTimer  Alert_Timer;          // 경계 타이머 (Max = 망각 후 경계 상태 유지 시간)

    [Header("공격")]
    [SerializeField] private float   AttackRange = 1.5f;  // 공격 거리 (NavMeshAgent StoppingDistance와 동일)
    [SerializeField] private FTimer  Attack_Timer;         // 공격 쿨다운 타이머 (Max = 공격 쿨다운 초)


    #region 상태
    [SerializeField] private EZombieState State = EZombieState.Idle;
    #endregion


    #region 컴포넌트
    private Character_Zombie Zombie;
    private NavMeshAgent     Agent;
    #endregion


    private void Awake()
    {
        Zombie = GetComponent<Character_Zombie>();

        Agent  = GetComponent<NavMeshAgent>();
        Agent.stoppingDistance = AttackRange;

        //테스트 용
        ContactObject.SetActive(false);
    }


    private void Update()
    {
        UpdateContact();

        switch (State)
        {
            // 대기
            case EZombieState.Idle:
                {
                    UpdateIdle();
                    break;
                }
            // 경계
            case EZombieState.Alert:
                {
                    UpdateAlert();
                    break;
                }
            // 추격
            case EZombieState.Chase:
                {
                    UpdateChase();
                    break;
                }
            // 공격
            case EZombieState.Attack:
                {
                    UpdateAttack();
                    break;
                }

        }

        // 매 프레임 현재 상태를 애니메이터에 반영
        Zombie.ZombieAnim.UpdateState(State);
    }

    // 컨택 타이머 갱신 및 상태 전환
    private void UpdateContact()
    {
        // 테스트 모드 — 상태를 Idle로 고정
        if (bTest)
        {
            if (State != EZombieState.Idle) ChangeState(EZombieState.Idle);
            return;
        }

        if (IsPlayerContact())
        {
            // 시야 안 — 컨택 타이머 누적, 망각 타이머 리셋
            Contact_Timer.Tick();
            Forget_Timer.Reset();
        }
        else if (Contact_Timer.IsReached)
        {
            // 컨택 상태에서 시야를 잃음 — 망각 타이머 누적
            Forget_Timer.Tick();
            if (Forget_Timer.IsReached)
            {
                // 망각 시간 초과 → 컨택 해제
                Contact_Timer.Reset();
                Forget_Timer.Reset();
            }
        }
        else
        {
            // 컨택 전 시야를 잃으면 즉시 리셋
            Contact_Timer.Reset();
        }

        ContactObject.SetActive(Contact_Timer.IsReached);

        // 컨택 여부에 따라 상태 전환
        if (Contact_Timer.IsReached && (State == EZombieState.Idle || State == EZombieState.Alert))
            ChangeState(EZombieState.Chase);
        else if (!Contact_Timer.IsReached && State == EZombieState.Chase)
            ChangeState(EZombieState.Alert);
    }

    // 플레이어가 감지 거리·각도 안에 있고 시야가 막히지 않으면 true
    private bool IsPlayerContact()
    {
        Vector3 toPlayer = Character_Player.Inst.transform.position - transform.position;

        // 거리 체크
        if (toPlayer.magnitude > ContactRange) return false;

        // 각도 체크 — 정면과 플레이어 방향 사이의 각도
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > ContactAngle / 2f) return false;

        // 레이캐스트 — 좀비 머리에서 플레이어 머리로, 장애물에 막히면 false
        Vector3 rayOrigin = Zombie.MeshComp.HeadBone.position;
        Vector3 rayTarget = Character_Player.Inst.MeshComp.HeadBone.position;
        Vector3 rayDir = (rayTarget - rayOrigin).normalized;
        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, ContactRange))
        {
            if (hit.collider.GetComponentInParent<Character_Player>() == null)
            {
                Debug.DrawRay(rayOrigin, rayDir * hit.distance, Color.red);
                return false;
            }
        }

        Debug.DrawRay(rayOrigin, rayTarget - rayOrigin, Color.green);
        return true;
    }

    // 대기 상태
    private void UpdateIdle()
    {

    }

    // 경계 상태 — Alert_Timer.Max 후 Idle로 복귀
    private void UpdateAlert()
    {
        Alert_Timer.Tick();
        if (Alert_Timer.IsReached)
            ChangeState(EZombieState.Idle);
    }

    // 추격 상태 — 플레이어 방향으로 이동, 공격 거리 진입 시 Attack 전환
    private void UpdateChase()
    {
        Agent.SetDestination(Character_Player.Inst.transform.position);

        float dist = Vector3.Distance(transform.position, Character_Player.Inst.transform.position);
        if (dist <= AttackRange)
            ChangeState(EZombieState.Attack);
    }

    // 공격 상태 — 쿨다운마다 플레이어에게 데미지, 사거리 이탈 시 Chase 복귀
    private void UpdateAttack()
    {
        float dist = Vector3.Distance(transform.position, Character_Player.Inst.transform.position);

        // 사거리 이탈 — Chase로 복귀
        if (dist > AttackRange)
        {
            ChangeState(EZombieState.Chase);
            return;
        }

        // 쿨다운마다 공격
        Attack_Timer.Tick();
        if (Attack_Timer.IsReached)
        {
            Debug.Log("좀비가 공격");
            Zombie.ZombieAnim.PlayAttack();
            Attack_Timer.Reset();
        }
    }

    private void ChangeState(EZombieState newState)
    {
        State = newState;
        switch (newState)
        {
            case EZombieState.Idle:
                Agent.ResetPath(); // 이동 멈춤
                break;
            case EZombieState.Alert:
                Agent.ResetPath();    // 이동 멈춤
                Alert_Timer.Reset();  // 경계 타이머 리셋
                break;
            case EZombieState.Chase:
                Alert_Timer.Reset();  // 경계 타이머 리셋
                break;
            case EZombieState.Attack:
                Attack_Timer.Current = Attack_Timer.Max; // 진입 즉시 첫 공격
                break;
        }
    }



#if UNITY_EDITOR
    // 씬 뷰에서 정면 기준 부채꼴 감지 영역 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float     halfAngle = ContactAngle / 2f;
        Vector3   forward   = transform.forward;
        Vector3   leftDir   = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3   rightDir  = Quaternion.Euler(0,  halfAngle, 0) * forward;

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
