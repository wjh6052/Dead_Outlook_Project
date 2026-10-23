using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬 파일 이름은 "열거형 이름 + _Scene" 규칙 (예: Hideout → Hideout_Scene)
public enum EScene
{
    Hideout, // 은신처
    Field,   // 필드
}

// 맵(씬) 관련 기능 모음 — Base_Scene에 배치
// - 씬 이름 변환, 씬 로드 · 언로드 기능만 제공하고, 언제 실행할지는 Mgr_Game.ChangeScene이 정함
public class Mgr_Map : MonoBehaviour
{
    [field: Tooltip("지금 플레이 중인 맵 — LoadMap으로 맵을 불러올 때 갱신")]
    [field: SerializeField] public EScene CurrentMap { get; private set; } // 현재 맵

    private Dictionary<EScene, MapController> MapControllers = new Dictionary<EScene, MapController>(); // 로드된 맵별 컨트롤러 — MapController.Start에서 등록
    private bool IsWaitingMap; // CurrentMap의 로드 완료(컨트롤러 등록)를 기다리는 중인지 — 준비 완료 알림이 한 번만 가도록

    public static Mgr_Map Inst { get; private set; } // 맵 매니저 싱글톤

    private void Awake()
    {
        Inst = this;
    }


    // 맵 컨트롤러 등록 — MapController.Start에서 호출 (씬 로드 완료 시점)
    // 기다리던 맵(CurrentMap)이면 Mgr_Game에 준비 완료 알림
    public void RegisterMap(MapController controller)
    {
        MapControllers[controller.Map] = controller;

        if (!IsWaitingMap || controller.Map != CurrentMap) return;
        IsWaitingMap = false;
        Mgr_Game.Inst.OnMapReady(controller);
    }



    // 맵 로드 — 이미 로드된 경우 중복 로드 방지
    public void LoadMap(EScene scene)
    {
        CurrentMap = scene; // 현재 맵 기록

        // 이미 로드 · 등록된 맵 (에디터에서 미리 열어두고 플레이 등) — 기다리지 않고 바로 준비 완료
        if (MapControllers.TryGetValue(scene, out MapController controller))
        {
            Mgr_Game.Inst.OnMapReady(controller);
            return;
        }

        IsWaitingMap = true; // 로드가 끝나 컨트롤러가 등록되면 준비 완료 알림
        LoadSceneIfNotLoaded(GetSceneName(scene));
    }

    // EScene → 씬 파일 이름 변환 (예: EScene.Hideout → "Hideout_Scene")
    public static string GetSceneName(EScene scene)
    {
        return scene + "_Scene";
    }

    // 씬이 로드되지 않은 경우에만 Additive 로드
    // static — Mgr_Game.Awake에서 UI · 플레이어 씬을 불러올 때 Mgr_Map.Inst가 아직 없을 수 있음
    public static void LoadSceneIfNotLoaded(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).IsValid()) return;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
    



    // 맵 언로드 — 로드돼 있을 때만
    public void UnloadMap(EScene scene)
    {
        string sceneName = GetSceneName(scene);
        if (!SceneManager.GetSceneByName(sceneName).isLoaded) return;

        // 언로드 전에 맵 나가기 신호 — 이 시점에는 씬 안의 오브젝트에 아직 접근 가능
        if (MapControllers.TryGetValue(scene, out MapController controller))
        {
            controller.OnMapExit();
            MapControllers.Remove(scene);
        }

        SceneManager.UnloadSceneAsync(sceneName);
    }

    // 로드된 필드 씬 전부 종료 — Hideout이 아닌 씬은 모두 필드로 취급
    public void UnloadFieldMaps()
    {
        foreach (EScene fieldScene in System.Enum.GetValues(typeof(EScene)))
        {
            if (fieldScene == EScene.Hideout) continue;
            UnloadMap(fieldScene); // 필드 종료
        }
    }
}
