using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Image IconBack_Image;  // 아이템 이미지
    private RectTransform Icon_Visual;      // 아이콘 비주얼 루트
    private Image Icon_Image;  // 아이템 이미지
    private TextMeshProUGUI Amount_TMP;  // 아이템 수량 텍스트

    public Item         ItemData    { get; private set; } // 아이템 데이터
    public UI_InventoryGrid Inventory  { get; private set; } // 소속 인벤토리
    private bool        IsEquipSlot;                     // 장착 슬롯 아이콘 여부

    // ── 드래그 ────────────────────────────────────────────────────────────────
    public static UI_ItemIcon BeingDragged { get; private set; } // 현재 드래그 중인 아이콘

    private Vector3          StartPosition;        // 드롭 실패 시 복귀 위치
    private Transform        StartParent;          // 드롭 실패 시 복귀 부모
    private UI_InventoryGrid     LastHoveredInventory; // 직전 프레임 호버 인벤토리 (하이라이트 초기화용)
    private UI_ItemSlot      LastHoveredSlot;      // 직전 프레임 호버 슬롯 (회전 시 하이라이트 갱신용)
    private GameObject       GhostIcon;            // 드래그 중 원위치에 표시되는 반투명 고스트
    private bool             bDraggingRotate;      // 드래그 중 회전 여부

    CanvasGroup CG; // 드래그 시 레이캐스트 제어용


    // UI_InventoryGrid.TryAddItem에서 아이콘 생성 시 호출
    public void Init(Item item, UI_InventoryGrid inventory, bool isEquipSlot = false)
    {
        FindComponents();

        ItemData    = item;
        Inventory   = inventory;
        IsEquipSlot = isEquipSlot;

        // 아이템 아이콘 설정
        Icon_Image.sprite  = item.Icon;

        // 등급별 색상 조정 (알파 0.5 반투명)
        Color rarityColor = Mgr_UI.Inst.GetRarityColor(item.Rarity);
        rarityColor.a = 0.3f;
        IconBack_Image.color = rarityColor;

        // 아이템 갯수 설정
        Amount_TMP.text    = item.Amount.ToString();

        SetRect(false);
    }

    // 컴포넌트 설정
    private void FindComponents()
    {
        if (IconBack_Image != null) return;

        IconBack_Image = GetComponent<Image>();

        Transform visual = transform.Find("Icon_Visual");
        Icon_Visual = visual.GetComponent<RectTransform>();
        Icon_Image = visual.Find("Icon_Image").GetComponent<Image>();
        Amount_TMP = visual.Find("Amount_TMP").GetComponent<TextMeshProUGUI>();

        CG = GetComponent<CanvasGroup>();
    }

    // 아이콘 RectTransform 위치 및 크기 설정
    public void SetRect(bool isDrag)
    {
        RectTransform rt = GetComponent<RectTransform>();

        if (IsEquipSlot)
        {
            // 장착 슬롯: 부모(Slot)에 꽉 채움
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.offsetMin        = Vector2.zero;
            rt.offsetMax        = Vector2.zero;

            Icon_Visual.anchorMin        = Vector2.zero;
            Icon_Visual.anchorMax        = Vector2.one;
            Icon_Visual.offsetMin        = Vector2.zero;
            Icon_Visual.offsetMax        = Vector2.zero;
            Icon_Visual.localRotation    = Quaternion.identity;
            return;
        }

        int sizeX = ItemData.IsRotated ? ItemData.TileSizeY : ItemData.TileSizeX;
        int sizeY = ItemData.IsRotated ? ItemData.TileSizeX : ItemData.TileSizeY;

        float cellSize = Mgr_UI.Inst.SlotCellSize;
        float spacing  = Mgr_UI.Inst.SlotSpacing;

        Vector2 anchor = isDrag ? new Vector2(0.5f, 0.5f) : new Vector2(0f, 1f);

        rt.pivot            = anchor;
        rt.anchorMin        = anchor;
        rt.anchorMax        = anchor;
        rt.sizeDelta        = new Vector2(sizeX * cellSize + (sizeX - 1) * spacing,
                                          sizeY * cellSize + (sizeY - 1) * spacing);
        rt.anchoredPosition = new Vector2( ItemData.SlotX * (cellSize + spacing),
                                          -ItemData.SlotY * (cellSize + spacing));

        // 회전 시 Icon_Visual 크기를 반전시켜 이미지가 올바르게 채워지도록
        float w = rt.sizeDelta.x;
        float h = rt.sizeDelta.y;
        Icon_Visual.sizeDelta     = ItemData.IsRotated ? new Vector2(h, w) : new Vector2(w, h);
        Icon_Visual.localRotation = Quaternion.Euler(0f, 0f, ItemData.IsRotated ? -90f : 0f);
    }

    // 드래그 중 아이템 90도 회전 — sizeDelta·비주얼 갱신 (피벗/위치 유지)
    public void Rotate()
    {
        ItemData.ToggleRotation();
        bDraggingRotate = !bDraggingRotate;

        int sizeX = ItemData.IsRotated ? ItemData.TileSizeY : ItemData.TileSizeX;
        int sizeY = ItemData.IsRotated ? ItemData.TileSizeX : ItemData.TileSizeY;

        float cellSize = Mgr_UI.Inst.SlotCellSize;
        float spacing  = Mgr_UI.Inst.SlotSpacing;
        float w = sizeX * cellSize + (sizeX - 1) * spacing;
        float h = sizeY * cellSize + (sizeY - 1) * spacing;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);

        // 회전 시 Icon_Visual 크기를 반전시켜 이미지가 올바르게 채워지도록
        Icon_Visual.sizeDelta     = ItemData.IsRotated ? new Vector2(h, w) : new Vector2(w, h);
        Icon_Visual.localRotation = Quaternion.Euler(0f, 0f, ItemData.IsRotated ? -90f : 0f);

        // 하이라이트 즉시 갱신
        if (LastHoveredInventory != null && LastHoveredSlot != null)
            LastHoveredInventory.HighlightSlots(LastHoveredSlot.GridX, LastHoveredSlot.GridY, ItemData);
    }

    // Amount_TMP 텍스트를 ItemData.Amount 기준으로 갱신
    public void UpdateAmount() => Amount_TMP.text = ItemData.Amount.ToString();

    // 드래그 중인 아이템이 가방이고, 해당 인벤토리가 자기 자신의 BagInventory인지 확인
    private bool IsOwnBagInventory(UI_InventoryGrid inventory)
        => ItemData is Item_Bag bag && inventory.Data == bag.BagInventory;

    // IBeginDragHandler — 드래그 시작 시 한 번 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeingDragged  = this;
        StartPosition = transform.position;
        StartParent   = transform.parent;

        // 원위치에 반투명 고스트 생성
        GhostIcon = Instantiate(gameObject, StartParent);
        CanvasGroup ghostCG = GhostIcon.GetComponent<CanvasGroup>();
        ghostCG.alpha          = 0.4f;
        ghostCG.blocksRaycasts = false;
        ghostCG.interactable   = false;

        SetRect(true);

        CG.blocksRaycasts = false;
        transform.SetParent(Mgr_UI.Inst.MainCanvas.transform);
    }

    // IDragHandler — 드래그 중 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        GetUIUnderCursor(eventData, out UI_InventoryGrid hoveredInventory, out UI_ItemSlot hoveredSlot);

        // 인벤토리가 바뀌었으면 이전 인벤토리 하이라이트 초기화
        if (LastHoveredInventory != null && LastHoveredInventory != hoveredInventory)
            LastHoveredInventory.ClearHighlights();

        // 슬롯 위에 있으면 하이라이트, 인벤토리 위지만 슬롯 밖이면 초기화
        if (hoveredInventory != null && hoveredSlot != null && !IsOwnBagInventory(hoveredInventory))
            hoveredInventory.HighlightSlots(hoveredSlot.GridX, hoveredSlot.GridY, ItemData);
        else if (hoveredInventory != null)
            hoveredInventory.ClearHighlights();

        LastHoveredInventory = hoveredInventory;
        LastHoveredSlot      = hoveredSlot;
    }

    // 커서 아래 UI_ItemSlot과 소속 UI_InventoryGrid를 탐색
    private void GetUIUnderCursor(PointerEventData eventData, out UI_InventoryGrid inventory, out UI_ItemSlot slot)
    {
        inventory = null;
        slot      = null;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            UI_ItemSlot found = result.gameObject.GetComponent<UI_ItemSlot>();
            if (found != null && found.Inventory != null)
            {
                slot      = found;
                inventory = found.Inventory;
                break;
            }
        }
    }

    

    // IEndDragHandler — 드래그 종료 시 한 번 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        BeingDragged = null;
        CG.blocksRaycasts = true;

        if (LastHoveredInventory != null) LastHoveredInventory.ClearHighlights();
        LastHoveredInventory = null;

        // 고스트 제거
        if (GhostIcon != null) Destroy(GhostIcon);

        // 드롭 위치 탐색
        GetUIUnderCursor(eventData, out UI_InventoryGrid hoveredInventory, out UI_ItemSlot hoveredSlot);

        if (hoveredInventory != null && hoveredSlot != null && !IsOwnBagInventory(hoveredInventory) &&
            hoveredInventory.TryDrop(ItemData, hoveredSlot.GridX, hoveredSlot.GridY, Inventory, this))
        {
            bDraggingRotate = false;
            Destroy(gameObject);
            return;
        }

        // 드롭 실패 또는 부분 스택 → 회전 복구 후 원래 자리로 복귀
        if (bDraggingRotate) Rotate();
        transform.position = StartPosition;
        transform.SetParent(StartParent);
        SetRect(false);
        bDraggingRotate = false;
    }

    

}
