using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum EGameState
{
    Play,  // 캐릭터를 움직이는 상태
    UI,    // 인벤토리, 메뉴 등 UI가 열린 상태
    Stop   // 사망 등 게임을 멈춰야 할 때
}

public enum EScene
{
    Hideout, // 은신처
    Field,   // 필드
}

public class Mgr_Game : MonoBehaviour
{

    [Header("씬 이름")]
    [SerializeField] public string Hideout_SceneName = "Hideout_Scene";
    [SerializeField] public string Field_SceneName   = "Field_Scene";

    [Header("GameState")]
    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.Play;


    public static Mgr_Game Inst { get; private set; }

    private void Awake()
    {
        Inst = this;

        // 이미 로드된 씬은 스킵 — 에디터 다중 씬 편집 환경과 빌드 모두 대응
        LoadSceneIfNotLoaded("UI_Scene");
        LoadSceneIfNotLoaded("Player_Scene");
    }

    private void Start()
    {
        ChangeScene(EScene.Hideout);
    }

    // 씬이 로드되지 않은 경우에만 Additive 로드


    private void Update()
    {

    }


    private void LoadSceneIfNotLoaded(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).IsValid()) return;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }


    // 씬 전환 — EScene으로 목적지 지정
    public void ChangeScene(EScene scene)
    {
        Mgr_UI.Inst.Reset();     // UI 초기화
        SetGameState(EGameState.Play); // 게임 상태 초기화

        switch (scene)
        {
            case EScene.Hideout:
                if (SceneManager.GetSceneByName(Field_SceneName).isLoaded)
                    SceneManager.UnloadSceneAsync(Field_SceneName); // 필드 종료
                Mgr_Hideout.Inst.SetHideout(true);                  // 은신처 로드
                break;

            case EScene.Field:
                Mgr_Hideout.Inst.SetHideout(false);             // 은신처 종료
                LoadSceneIfNotLoaded(Field_SceneName);          // 필드 로드
                break;
        }
    }

    public void SetGameState(EGameState newGameState)
    {
        GameState = newGameState;

        switch (GameState)
        {
            case EGameState.Play:
                {
                    Mgr_Camera.Inst.SetCursorLock(true);
                }
                break;
            case EGameState.UI:
                {
                    Mgr_Camera.Inst.SetCursorLock(false);
                }
                break;
            case EGameState.Stop:
                {

                }
                break;
        }
    }

    
}
