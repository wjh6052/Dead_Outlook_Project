using UnityEngine;

// 필드 맵 컨트롤러 — Field_Scene에 배치, 필드에 들어올 때 · 나갈 때 할 일
public class MapController_Field : MapController
{
    // 필드에 들어왔을 때 — 스폰 이동 후 호출
    public override void OnMapEnter()
    {
        Debug.Log("[MapController_Field] 필드 진입");
        base.OnMapEnter();
    }

    // 필드를 나갈 때 — 언로드 직전 호출
    public override void OnMapExit()
    {
        Debug.Log("[MapController_Field] 필드 나감");
        base.OnMapExit();
    }
}
