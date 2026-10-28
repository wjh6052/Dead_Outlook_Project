using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 파일 이름 = 열거형 이름 + "_Scene" (예: Hideout → Hideout_Scene)
public enum EScene
{
    Hideout, // 은신처
    Field,   // 필드
}

// 맵 매니저 — 맵 씬 로드 · 언로드 기능 제공 (실행 시점은 Mgr_Game이 정함)
public class Mgr_Map : MonoBehaviour
{
    public MapController CurrentMap { get; private set; } // 들어가 있는 맵 (null = 없음)

    // (추가 예정) 은신처별 데이터 저장

    public static Mgr_Map Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }


    #region 로드

    // 맵 로드
    public void LoadMap(EScene scene)
    {
        LoadSceneIfNotLoaded(GetSceneName(scene));
    }

    // 맵 등록 — MapController에서 호출
    public void RegisterMap(MapController controller)
    {
        CurrentMap = controller;
    }

    #endregion


    #region 언로드

    // 지금 맵 언로드 — 언로드 전에 OnMapExit 호출 (CurrentMap 비우기는 UnregisterMap에서)
    public void UnloadMap()
    {
        if (CurrentMap == null) return; // 켜진 맵 없음

        CurrentMap.OnMapExit();
        SceneManager.UnloadSceneAsync(CurrentMap.gameObject.scene);
    }

    // 맵 해제 — MapController에서 호출
    public void UnregisterMap(MapController controller)
    {
        if (CurrentMap == controller) CurrentMap = null;
    }

    #endregion


    #region 유틸

    // EScene → 씬 이름
    public static string GetSceneName(EScene scene)
    {
        return scene + "_Scene";
    }

    // 씬이 켜져 있지 않을 때만 Additive 로드
    public static void LoadSceneIfNotLoaded(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded) return; // 완전히 켜진 씬만 건너뜀
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    #endregion
}
