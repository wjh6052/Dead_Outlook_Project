using UnityEngine;

// 맵 컨트롤러 공통 부모 — 각 맵 씬에 하나씩 배치
// - 씬이 로드되면 스스로 Mgr_Map에 등록하고, 맵에 들어올 때 · 나갈 때 신호를 받음
public class MapController : MonoBehaviour
{
    [field: Tooltip("이 컨트롤러가 속한 맵")]
    [field: SerializeField] public EScene Map { get; private set; } // 이 컨트롤러가 속한 맵

    [Header("스폰 위치")]
    [Tooltip("이 맵에 들어올 때 플레이어가 나타날 위치 (월드 좌표)")]
    public Vector3 SpawnPosition; // 스폰 위치
    [Tooltip("스폰 방향 (오일러 각도 — 보통 Y값으로 바라볼 방향만 지정)")]
    public Vector3 SpawnRotation; // 스폰 방향

    // 씬 로드가 끝나면 호출됨 — Mgr_Map에 등록 (가려던 맵이면 준비 완료 처리)
    protected virtual void Start()
    {
        Mgr_Map.Inst.RegisterMap(this);
    }

    // 맵에 들어왔을 때 — Mgr_Game.OnMapReady에서 호출 (자식 클래스에서 덮어쓰기, base 호출 필수)
    public virtual void OnMapEnter()
    {
        // 이 맵의 스폰 위치로 플레이어 이동 — 씬 로드가 끝난 뒤라 바닥이 있는 상태
        Character_Player.Inst.Movement.Teleport(SpawnPosition, Quaternion.Euler(SpawnRotation));
    }

    // 맵을 나갈 때 (언로드 직전) — Mgr_Map.UnloadMap에서 호출 (자식 클래스에서 덮어쓰기)
    public virtual void OnMapExit() { }
}
