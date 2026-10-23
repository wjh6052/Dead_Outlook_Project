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

public class Mgr_Game : MonoBehaviour
{

    [Header("GameState")]
    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.Play;

    [Header("사망")]
    [Tooltip("사망 후 은신처로 귀환하기까지 기다리는 시간 (초) — 사망 애니메이션이 보이는 시간")]
    [SerializeField] private float DeathDelay = 5f; // 사망 후 귀환까지 대기 시간


    public static Mgr_Game Inst { get; private set; }

    private void Awake()
    {
        Inst = this;

        // 이미 로드된 씬은 스킵 — 에디터 다중 씬 편집 환경과 빌드 모두 대응
        Mgr_Map.LoadSceneIfNotLoaded("UI_Scene");
        Mgr_Map.LoadSceneIfNotLoaded("Player_Scene");
    }

    private void Start()
    {
        ChangeScene(EScene.Hideout);
    }


    private void Update()
    {

    }


    // 씬 전환 — EScene으로 목적지 지정, 실제 씬 로드 · 언로드는 Mgr_Map 기능으로 실행
    public void ChangeScene(EScene scene)
    {
        Mgr_UI.Inst.Reset();     // UI 초기화
        SetGameState(EGameState.Play); // 게임 상태 초기화

        switch (scene)
        {
            case EScene.Hideout:
                Mgr_Map.Inst.UnloadFieldMaps();        // 로드된 필드 씬 전부 종료
                Mgr_Map.Inst.LoadMap(EScene.Hideout);  // 은신처 로드
                break;

            // Hideout이 아닌 씬은 모두 필드로 취급
            default:
                Mgr_Map.Inst.UnloadMap(EScene.Hideout); // 은신처 종료
                Mgr_Map.Inst.LoadMap(scene);            // 필드 로드
                break;
        }
    }

    // 맵 준비 완료 — Mgr_Map이 맵 로드 완료(MapController 등록)를 알릴 때 호출
    public void OnMapReady(MapController map)
    {
        map.OnMapEnter(); // 맵 진입 신호
    }


    // 플레이어 사망 — Character_Player.OnDeath에서 호출, 사망 후 게임 흐름 처리
    public void PlayerDeath()
    {
        SetGameState(EGameState.Stop);          // 조작 · 카메라 잠금 — 사망 애니메이션이 재생되는 동안
        StartCoroutine(PlayerDeathCoroutine()); // DeathDelay초 뒤 귀환 처리
    }

    // 사망 후 DeathDelay초 기다렸다가 아이템 손실 · 은신처 귀환
    private IEnumerator PlayerDeathCoroutine()
    {
        yield return new WaitForSeconds(DeathDelay);

        Character_Player.Inst.InventoryComp.ClearInventory(); // 보유 아이템 전체 손실
        ChangeScene(EScene.Hideout);                          // 은신처로 귀환
        Character_Player.Inst.Revive();                       // 부활 — 체력 · 자세 복구
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
