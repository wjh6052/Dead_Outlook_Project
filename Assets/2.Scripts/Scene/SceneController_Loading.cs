using System.Collections;
using TMPro;
using UnityEngine;

// 로딩 씬 컨트롤러 — 로딩창 화면, Mgr_Game에 등록
public class SceneController_Loading : SceneController
{
    [Tooltip("진행 중인 작업 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI Message_TMP;

    // 준비 작업
    protected override IEnumerator C_Prepare()
    {
        yield return base.C_Prepare();
    }

    // Mgr_Game에 등록 — C_Prepare가 끝난 후 호출
    protected override void Register()
    {
        Mgr_Game.Inst.SetSceneLoading(this);
    }

    // Mgr_Game에서 해제
    protected override void Unregister()
    {
        if (Mgr_Game.Inst != null) Mgr_Game.Inst.SetSceneLoading(null); // 종료 시 매니저가 먼저 사라질 수 있음
    }

    // 로딩 글자 변경 — 연결이 없으면 같은 씬에서 "Message_TMP" 이름으로 찾음
    public void SetMessage(string text)
    {
        // 같은 씬에서 이름이 Message_TMP인 텍스트 찾기 (꺼진 오브젝트 포함)
        if (Message_TMP == null)
        {
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                foreach (TextMeshProUGUI tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (tmp.name == "Message_TMP") Message_TMP = tmp;
        }

        if (Message_TMP == null)
        {
            Debug.LogError("[SceneController_Loading] Message_TMP를 찾지 못함");
            return;
        }

        Message_TMP.text = text;
    }
}
