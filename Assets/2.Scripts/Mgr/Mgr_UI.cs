using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mgr_UI : MonoBehaviour
{
    #region 슬롯 색상 설정값
    [Header("슬롯 하이라이트 색상")] // Default / Valid / Invalid / Partial 순서
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

    #endregion

    [Header("슬롯 설정")]
    [SerializeField] public float SlotCellSize = 63f;
    [SerializeField] public float SlotSpacing  = 2f;

    [Header("변수")]
    public bool IsOnUI = false;
    


    public Canvas MainCanvas { get; private set; } // 메인 UI 캔버스
    public UI_Inventory Inventory_Panel { get; private set; }       // 인벤토리 패널

    private UI_ContextMenu ContextMenuPrefab; // 컨텍스트 메뉴 프리팹

    #region UI 프리팹
    public UI_ItemSlot  SlotPrefab      { get; private set; } // 슬롯 프리팹
    public UI_ItemIcon  IconPrefab      { get; private set; } // 아이콘 프리팹

    #endregion

    public static Mgr_UI Inst { get; private set; }

    private void Awake()
    {
        Inst              = this;
        MainCanvas        = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
        Inventory_Panel   = MainCanvas.transform.Find("UI_Inventory").GetComponent<UI_Inventory>();

        //UI 프리팹
        {
            SlotPrefab = Resources.Load<UI_ItemSlot>("Prefab/UI/Inventory/PF_UI_Item_Slot");
            IconPrefab = Resources.Load<UI_ItemIcon>("Prefab/UI/Inventory/PF_UI_Item_Icon");
            ContextMenuPrefab = Resources.Load<UI_ContextMenu>("Prefab/UI/Inventory/PF_UI_ContextMenu");
        }

    }



    private IEnumerator Start()
    {
        yield return null; // 한 프레임 대기
        OnInventory(false);
    }

    private void Update()
    {

    }



    // 인벤토리 열기 닫기
    public void OnInventory(bool isOn)
    {
        IsOnUI = isOn;
        Inventory_Panel.gameObject.SetActive(isOn);
        if (IsOnUI == true) Inventory_Panel.Init();


    }

    // 드래그중인 아이템 회전
    public void OnRotateItem()
    {
        if (UI_ItemIcon.BeingDragged == null) return;
        UI_ItemIcon.BeingDragged.Rotate();
    }


    // 컨텍스트 메뉴 표시 — 프리팹 스폰 후 Show 호출


    

}
