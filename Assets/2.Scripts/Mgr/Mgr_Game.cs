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

// 씬 파일 이름은 "열거형 이름 + _Scene" 규칙 (예: Hideout → Hideout_Scene)
public enum EScene
{
    Hideout, // 은신처
    Field,   // 필드
}

public class Mgr_Game : MonoBehaviour
{

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


    // EScene → 씬 파일 이름 변환 (예: EScene.Hideout → "Hideout_Scene")
    public static string GetSceneName(EScene scene)
    {
        return scene + "_Scene";
    }

    // 씬 전환 — EScene으로 목적지 지정
    public void ChangeScene(EScene scene)
    {
        Mgr_UI.Inst.Reset();     // UI 초기화
        SetGameState(EGameState.Play); // 게임 상태 초기화

        switch (scene)
        {
            case EScene.Hideout:
                // 로드된 필드 씬 전부 종료
                foreach (EScene fieldScene in System.Enum.GetValues(typeof(EScene)))
                {
                    if (fieldScene == EScene.Hideout) continue;
                    if (SceneManager.GetSceneByName(GetSceneName(fieldScene)).isLoaded)
                        SceneManager.UnloadSceneAsync(GetSceneName(fieldScene)); // 필드 종료
                }
                Mgr_Hideout.Inst.SetHideout(true);                  // 은신처 로드
                break;

            // Hideout이 아닌 씬은 모두 필드로 취급
            default:
                Mgr_Hideout.Inst.SetHideout(false);             // 은신처 종료
                LoadSceneIfNotLoaded(GetSceneName(scene));      // 필드 로드
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
                    Mgr_Camera.Inst.SetCursorLock(true); // 조작 불가 상태 — UI가 없으니 커서 숨김
                }
                break;
        }
    }

    
}
