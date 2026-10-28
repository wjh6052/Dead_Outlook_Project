using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public enum EGameState
{
    Play,   // 플레이 중
    Loading // 로딩 중
}

public class Mgr_Game : MonoBehaviour
{

    [Header("GameState")]
    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.Play;

    [Header("사망")]
    [Tooltip("사망 후 은신처 귀환까지 대기 시간 (초)")]
    [SerializeField] private float DeathDelay = 5f; // 사망 후 귀환 대기 시간

    private float         LoadingDelay = 0.2f; // 단계별 대기 시간 (초) — 테스트용

    public SceneController_Loading SceneLoading { get; private set; } // 로딩창
    public void SetSceneLoading(SceneController_Loading loading) { SceneLoading = loading; }

    public static Mgr_Game Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    private void Start()
    {
        StartCoroutine(C_GameStart());
    }

    // 게임 시작 — 준비 후 첫 맵으로 이동
    private IEnumerator C_GameStart()
    {
        GameState = EGameState.Loading; // 조작 잠금 (커서 처리는 3-1 이후 SetGameState에서)


        #region 1. Mgr_Scene 외의 씬 정리

        // 미리 열린 씬을 모두 끄고 3번에서 순서대로 다시 로드
        List<AsyncOperation> unloads = new List<AsyncOperation>();
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene == gameObject.scene) continue; // Mgr_Scene 유지
            if (!scene.isLoaded) continue;           // 꺼진 씬 건너뜀
            unloads.Add(SceneManager.UnloadSceneAsync(scene));
        }
        foreach (AsyncOperation unload in unloads)
            yield return unload;                     // 언로드 대기

        #endregion


        #region 2. 로딩창 켜기

        SetLoadingScene(true); // 로딩창 씬 켜기
        yield return new WaitUntil(() => SceneLoading != null); // 로딩창 등록 대기

        #endregion


        #region 3. 매니저 준비 확인

        // 3-1. 카메라 — 확인 후 Camera_Scene 로드
        {
            yield return C_LoadingMessage("카메라 준비 중");
            yield return new WaitUntil(() => Mgr_Camera.Inst != null);
            Mgr_Map.LoadSceneIfNotLoaded("Camera_Scene");
            yield return new WaitUntil(() => Mgr_Camera.Inst.CameraController != null); // 카메라 등록 대기
            SetGameState(EGameState.Loading); // 카메라 준비 후 상태 처리 (커서 잠금)
        }

        // 3-2. 아이템
        {
            yield return C_LoadingMessage("아이템 준비 중");
            yield return new WaitUntil(() => Mgr_Item.Inst != null);
        }

        // 3-3. 은신처
        {
            yield return C_LoadingMessage("은신처 준비 중");
            yield return new WaitUntil(() => Mgr_Hideout.Inst != null);
        }

        // 3-4. 입력
        {
            yield return C_LoadingMessage("입력 준비 중");
            yield return new WaitUntil(() => Mgr_Input.Inst != null);
        }

        // 3-5. 플레이어 컨트롤러 — 확인 후 Player_Scene 로드
        {
            yield return C_LoadingMessage("플레이어 준비 중");
            yield return new WaitUntil(() => Mgr_PlayerController.Inst != null);
            Mgr_Map.LoadSceneIfNotLoaded("Player_Scene");
            yield return new WaitUntil(() => Character_Player.Inst != null); // 플레이어 생성 대기
            Mgr_PlayerController.Inst.SetFreeze(true);                       // 플레이어 고정
            Mgr_Camera.Inst.SetTarget(Character_Player.Inst.CameraTarget);  // 카메라를 플레이어에게 설정
        }

        // 3-6. UI — 확인 후 UI_Scene 로드
        {
            yield return C_LoadingMessage("UI 준비 중");
            yield return new WaitUntil(() => Mgr_UI.Inst != null);
            Mgr_Map.LoadSceneIfNotLoaded("UI_Scene");
            yield return new WaitUntil(() => Mgr_UI.Inst.SceneUI != null); // UI 등록 대기
        }

        // 3-7. 소음
        {
            yield return C_LoadingMessage("소음 준비 중");
            yield return new WaitUntil(() => Mgr_Noise.Inst != null);
        }

        // 3-8. 맵
        {
            yield return C_LoadingMessage("맵 준비 중");
            yield return new WaitUntil(() => Mgr_Map.Inst != null);
        }

        #endregion


        #region 4. 첫 맵으로 이동

        yield return StartCoroutine(C_LoadMap(EScene.Hideout)); // 첫 맵 로드 · 진입까지 대기

        #endregion


        #region 5. 시작 대기

        yield return C_WaitAnyButton(); // 아무 키나 눌러 시작

        #endregion


        #region 6. 로딩창 끄기

        SetLoadingScene(false);        // 로딩창 씬 끄기
        yield return new WaitUntil(() => SceneLoading == null); // 로딩창 해제 대기
        Mgr_PlayerController.Inst.SetFreeze(false); // 플레이어 고정 해제
        SetGameState(EGameState.Play); // 조작 잠금 해제

        #endregion

    }


    // 씬 전환 — 전환 중이면 무시
    public void ChangeScene(EScene scene)
    {
        if (GameState == EGameState.Loading) return; // 0. 중복 전환 방지
        StartCoroutine(C_ChangeScene(scene));
    }

    // 씬 전환 — 로딩 시작 → 조작 설정 → 맵 바꾸기 → 시작 대기 → 로딩 끝
    private IEnumerator C_ChangeScene(EScene scene)
    {
        // 1. 로딩 시작
        SetGameState(EGameState.Loading);                        // 조작 잠금
        SetLoadingScene(true);                                   // 로딩창 켜기
        yield return new WaitUntil(() => SceneLoading != null);  // 로딩창 등록 대기

        // 2. 조작 설정
        yield return C_LoadingMessage("조작 설정 중");            // 로딩 글자
        Mgr_PlayerController.Inst.SetFreeze(true);               // 플레이어 고정
        Mgr_UI.Inst.Reset();                                     // UI 초기화
        Mgr_PlayerController.Inst.SetPlayState(EPlayState.Play); // 조작 상태 초기화 (UI 닫힘 · 사망 후 귀환)

        // 3. 맵 바꾸기
        yield return StartCoroutine(C_LoadMap(scene));

        // 4. 시작 대기
        yield return C_WaitAnyButton();                          // 아무 키나 눌러 시작

        // 5. 로딩 끝
        SetLoadingScene(false);                                  // 로딩창 끄기
        yield return new WaitUntil(() => SceneLoading == null);  // 로딩창 해제 대기
        Mgr_PlayerController.Inst.SetFreeze(false);              // 플레이어 고정 해제
        SetGameState(EGameState.Play);                           // 조작 잠금 해제
    }

    // 맵 바꾸기 — 이전 맵 언로드 → 목적지 맵 로드 → 진입 (C_ChangeScene · C_GameStart에서 호출)
    private IEnumerator C_LoadMap(EScene scene)
    {
        // 1. 이전 맵 정리
        Mgr_Map.Inst.UnloadMap();                                          // 이전 맵 언로드
        yield return C_LoadingMessage("맵 정리 중");                        // 로딩 글자
        yield return new WaitUntil(() => Mgr_Map.Inst.CurrentMap == null); // 언로드 대기


        // 2. 목적지 맵 준비
        Mgr_Map.Inst.LoadMap(scene);                                       // 목적지 맵 로드
        yield return C_LoadingMessage("맵 불러오는 중");                    // 로딩 글자
        yield return new WaitUntil(() => Mgr_Map.Inst.CurrentMap != null); // 로드 대기


        // 3. 맵 진입
        Mgr_Map.Inst.CurrentMap.OnMapEnter();                              // 스폰
    }




    // 플레이어 사망 — Character_Player.OnDeath에서 호출
    public void PlayerDeath()
    {
        Mgr_PlayerController.Inst.SetPlayState(EPlayState.Dead); // 조작 잠금
        StartCoroutine(C_PlayerDeath()); // 귀환 처리
    }

    // 사망 후 대기 → 아이템 손실 · 은신처 귀환
    private IEnumerator C_PlayerDeath()
    {
        yield return new WaitForSeconds(DeathDelay);

        Character_Player.Inst.InventoryComp.ClearInventory(); // 아이템 손실
        ChangeScene(EScene.Hideout);                          // 은신처 귀환
        Character_Player.Inst.Revive();                       // 부활
    }

    // 게임 상태 변경 — 상태별 커서 · 로딩창 처리
    public void SetGameState(EGameState newGameState)
    {
        GameState = newGameState;

        switch (GameState)
        {
            case EGameState.Play:
                {
                    Mgr_Camera.Inst.SetCursorLock(true); // 커서 숨김
                }
                break;
            case EGameState.Loading:
                {
                    Mgr_Camera.Inst.SetCursorLock(false); // 커서 보임
                }
                break;
        }
    }

    // 로딩창 씬 켜기 · 끄기 (true: 켜기 / false: 끄기)
    public void SetLoadingScene(bool isOn)
    {
        if (isOn)
        {
            Mgr_Map.LoadSceneIfNotLoaded("Loading_Scene");
        }
        else
        {
            Scene loadingScene = SceneManager.GetSceneByName("Loading_Scene");
            if (loadingScene.isLoaded) SceneManager.UnloadSceneAsync(loadingScene);
        }
    }

    // 로딩 글자 변경 후 LoadingDelay만큼 대기 (skipDelay가 true면 대기 없음)
    private IEnumerator C_LoadingMessage(string text, bool skipDelay = false)
    {
        SceneLoading.SetMessage(text);
        if (skipDelay) yield break;
        yield return new WaitForSeconds(LoadingDelay);
    }

    // 아무 버튼이나 눌러 시작 — 안내 표시 후 새로 눌린 버튼이 있을 때까지 대기
    private IEnumerator C_WaitAnyButton()
    {
        yield return C_LoadingMessage("아무 키나 눌러 시작", true); // 대기 없이 바로 감지 시작

        bool isPressed = false;
        InputSystem.onAnyButtonPress.CallOnce(_ => isPressed = true); // 이 시점 이후 눌린 버튼 하나만 받음
        yield return new WaitUntil(() => isPressed);
    }

}
