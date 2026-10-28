using System.Collections;
using UnityEngine;

// 은신처 맵 컨트롤러
public class MapController_Hideout : MapController
{
    // 준비 작업 (은신처 — 오브젝트 · 아이템 배치 등 추가 예정)
    protected override IEnumerator C_Prepare()
    {
        yield return base.C_Prepare();
    }

    // 은신처 진입
    public override void OnMapEnter()
    {
        //Debug.Log("[MapController_Hideout] 은신처 진입");
        base.OnMapEnter();
    }

    // 은신처 나감
    public override void OnMapExit()
    {
        //Debug.Log("[MapController_Hideout] 은신처 나감");
        base.OnMapExit();
    }
}
