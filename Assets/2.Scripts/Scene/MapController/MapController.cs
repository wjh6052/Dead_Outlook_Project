using System.Collections;
using UnityEngine;

// 맵 컨트롤러 공통 부모 — Mgr_Map에 등록, 맵 진입 · 나감 처리
public class MapController : SceneController
{
    [field: Tooltip("이 컨트롤러가 속한 맵")]
    [field: SerializeField] public EScene Map { get; private set; } // 속한 맵

    [Header("스폰 위치")]
    [Tooltip("플레이어 스폰 위치 (월드 좌표)")]
    public Vector3 SpawnPosition; // 스폰 위치
    [Tooltip("스폰 방향 (오일러 각도)")]
    public Vector3 SpawnRotation; // 스폰 방향

    // 준비 작업 (맵 공통)
    protected override IEnumerator C_Prepare()
    {
        yield return base.C_Prepare();
    }

    // Mgr_Map에 등록 — C_Prepare가 끝난 후 호출
    protected override void Register()
    {
        Mgr_Map.Inst.RegisterMap(this);
    }

    // Mgr_Map에서 해제
    protected override void Unregister()
    {
        if (Mgr_Map.Inst != null) Mgr_Map.Inst.UnregisterMap(this); // 종료 시 매니저가 먼저 사라질 수 있음
    }

    // 맵 진입 — 스폰 위치로 이동 (자식은 base 호출 필수)
    public virtual void OnMapEnter()
    {
        Character_Player.Inst.Movement.Teleport(SpawnPosition, Quaternion.Euler(SpawnRotation));
    }

    // 맵 나감 (언로드 직전)
    public virtual void OnMapExit() { }
}
