using System.Collections;
using UnityEngine;

// 필드 맵 컨트롤러
public class MapController_Field : MapController
{
    // 준비 작업 (필드)
    protected override IEnumerator C_Prepare()
    {
        yield return base.C_Prepare();
    }

    // 필드 진입
    public override void OnMapEnter()
    {
        //Debug.Log("[MapController_Field] 필드 진입");
        base.OnMapEnter();
    }

    // 필드 나감
    public override void OnMapExit()
    {
        //Debug.Log("[MapController_Field] 필드 나감");
        base.OnMapExit();
    }
}
