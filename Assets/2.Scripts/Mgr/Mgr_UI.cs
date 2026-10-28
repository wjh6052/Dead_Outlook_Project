using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mgr_UI : MonoBehaviour
{
    #region 슬롯 설정값

    [Header("슬롯 하이라이트 색상")]
    [SerializeField]
    public Color[] SlotColors = new Color[]
    {
        new Color(1.00f, 1.00f, 1.00f, 0.10f), // Default — 기본
        new Color(0.00f, 1.00f, 0.00f, 0.50f), // Valid   — 배치 가능 (초록)
        new Color(1.00f, 0.00f, 0.00f, 0.50f), // Invalid — 배치 불가 (빨강)
        new Color(1.00f, 1.00f, 0.00f, 0.50f), // Partial — 예비 (노랑)
    };
    public Color GetHighlightColor(ESlotHighlight highlight) => SlotColors[(int)highlight];


    [Header("희귀도 색상")] // Normal / Rare / Epic / Unique / Legendary 순서
    [SerializeField]
    public Color[] RarityColors = new Color[]
    {
        new Color(1.00f, 1.00f, 1.00f), // Normal    — 흰색
        new Color(0.53f, 0.81f, 0.98f), // Rare      — 하늘색
        new Color(0.80f, 0.60f, 1.00f), // Epic      — 연보라
        new Color(1.00f, 1.00f, 0.60f), // Unique    — 연노랑
        new Color(0.56f, 0.93f, 0.56f), // Legendary — 연초록
    };
    public Color GetRarityColor(ERarity rarity) => RarityColors[(int)rarity];


    [Header("슬롯 설정")]
    [SerializeField] public float SlotCellSize = 63f;
    [SerializeField] public float SlotSpacing = 2f;

    #endregion


    [Header("변수")]
    public bool IsOnUI = false;


    #region UI_Scene

    // UI_Scene의 UI 참조 — SceneController_UI가 보관, Mgr_UI는 전달만 (RegisterUI에서 등록)
    public SceneController_UI SceneUI { get; private set; } // UI_Scene 컨트롤러

    public Canvas MainCanvas => SceneUI.MainCanvas; // 메인 UI 캔버스
    public UI_FocusTimer  FocusTimer => SceneUI.FocusTimer; // 집중 타이머 UI
    public UI_EscapeTimer EscapeTimer => SceneUI.EscapeTimer; // 탈출 타이머 UI
    public UI_Inventory UI_Inventory_Player => SceneUI.UI_Inventory_Player;  // 플레이어 인벤토리UI          // 인벤토리 패널

    #endregion

    private UI_ContextMenu  CurrentContextMenu; // 현재 열린 컨텍스트 메뉴
    private List<UI_Window> OpenWindows = new List<UI_Window>(); // 열려있는 창 목록

    #region UI 프리팹
    public UI_ItemSlot  SlotPrefab      { get; private set; } // 슬롯 프리팹
    public UI_ItemIcon  IconPrefab      { get; private set; } // 아이콘 프리팹
    private UI_Window   WindowPrefab;                         // 창 프리팹

    public WUI_Interact InteractWUI { get; private set; }

    #endregion


    public static Mgr_UI Inst { get; private set; }


    private void Awake()
    {
        Inst = this;

        // 프리팹
        {
            InteractWUI = Instantiate(Resources.Load<WUI_Interact>("Prefab/WUI/PF_WUI_Interact"));
            SlotPrefab   = Resources.Load<UI_ItemSlot>("Prefab/UI/Inventory/PF_UI_Item_Slot");
            IconPrefab   = Resources.Load<UI_ItemIcon>("Prefab/UI/Inventory/PF_UI_Item_Icon");
            WindowPrefab = Resources.Load<UI_Window>("Prefab/UI/Window/PF_UI_Window");
        }
    }



    // UI 등록 + 초기화 — SceneController_UI.Register에서 호출
    public void RegisterUI(SceneController_UI ui)
    {
        SceneUI = ui;

        FocusTimer.gameObject.SetActive(false);
        EscapeTimer.Init(false);

        if (UI_Inventory_Player.gameObject.activeSelf)
            UI_Inventory_Player.gameObject.SetActive(false);

        UI_Inventory_Player.Setup();
    }


    // 씬 전환 시 모든 UI 초기화 — Mgr_Game.ChangeScene에서 호출
    public void Reset()
    {
        OnInventory(false);

        if (CurrentContextMenu != null)
        {
            Destroy(CurrentContextMenu.gameObject);
            CurrentContextMenu = null;
        }

        ShowInteractUI(false);
    }

    // 인벤토리 열기 닫기
    public void OnInventory(bool isOn)
    {
        IsOnUI = isOn;
        UI_Inventory_Player.gameObject.SetActive(isOn);
        if (IsOnUI == true) UI_Inventory_Player.Init();
        else
        {
            foreach (UI_Window window in OpenWindows)
                if (window != null) Destroy(window.gameObject);
            OpenWindows.Clear();
        }
    }

    // 드래그중인 아이템 회전
    public void OnRotateItem()
    {
        if (UI_ItemIcon.BeingDragged == null) return;
        UI_ItemIcon.BeingDragged.Rotate();
    }


    // 상호작용 UI 표시 / 숨김
    public void ShowInteractUI(bool show, Interactable target = null, string interactText = "")
    {
        InteractWUI?.Show(show, target, interactText);
    }

    // 컨텍스트 메뉴 표시 — 기존 메뉴 닫고 새로 스폰
    public void ShowContextMenu(UI_ItemIcon icon, Vector2 position)
    {
        if (CurrentContextMenu != null) Destroy(CurrentContextMenu.gameObject);
        UI_ContextMenu prefab = Resources.Load<UI_ContextMenu>("Prefab/UI/Inventory/PF_UI_ContextMenu");
        CurrentContextMenu = Instantiate(prefab, MainCanvas.transform);
        CurrentContextMenu.Init(icon, position);
    }

    // 창 스폰
    public void SpawnWindow(string title, IUI_WindowContent content, bool addToOpenWindows = true)
    {
        UI_Window window = Instantiate(WindowPrefab, MainCanvas.transform);
        window.Init(title, content);
        if (addToOpenWindows) OpenWindows.Add(window);
    }

    
}
