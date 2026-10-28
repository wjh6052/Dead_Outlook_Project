using System.Collections;
using UnityEngine;

// UI 씬 컨트롤러 — UI 참조를 모아 Mgr_UI에 전달
public class SceneController_UI : SceneController
{
    [Tooltip("메인 UI 캔버스")]
    public Canvas         MainCanvas;
    [Tooltip("집중 타이머 UI")]
    public UI_FocusTimer  FocusTimer;
    [Tooltip("탈출 타이머 UI")]
    public UI_EscapeTimer EscapeTimer;
    [Tooltip("플레이어 인벤토리 UI")]
    public UI_Inventory   UI_Inventory_Player;

    // 준비 작업
    protected override IEnumerator C_Prepare()
    {
        yield return base.C_Prepare();

        // UI 참조 확인
        if (MainCanvas          == null) Debug.LogError("[SceneController_UI] MainCanvas가 연결되지 않음");
        if (FocusTimer          == null) Debug.LogError("[SceneController_UI] FocusTimer가 연결되지 않음");
        if (EscapeTimer         == null) Debug.LogError("[SceneController_UI] EscapeTimer가 연결되지 않음");
        if (UI_Inventory_Player == null) Debug.LogError("[SceneController_UI] UI_Inventory_Player가 연결되지 않음");
    }

    // Mgr_UI에 등록 — C_Prepare가 끝난 후 호출
    protected override void Register()
    {
        Mgr_UI.Inst.RegisterUI(this);
    }
}
