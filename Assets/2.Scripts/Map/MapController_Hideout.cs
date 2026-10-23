using UnityEngine;

// 은신처 맵 컨트롤러 — Hideout_Scene에 배치, 은신처에 들어올 때 · 나갈 때 할 일
public class MapController_Hideout : MapController
{
    // 은신처에 들어왔을 때 — 스폰 이동 후 호출
    public override void OnMapEnter()
    {
        Debug.Log("[MapController_Hideout] 은신처 진입");
        base.OnMapEnter();
    }

    // 은신처를 나갈 때 — 언로드 직전 호출
    public override void OnMapExit()
    {
        Debug.Log("[MapController_Hideout] 은신처 나감");
        base.OnMapExit();
    }
}
