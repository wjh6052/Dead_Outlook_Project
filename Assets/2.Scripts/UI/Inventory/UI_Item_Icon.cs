using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Item_Icon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    // ── Inspector ────────────────────────────────────────────────────────────
    [SerializeField] private Image           Icon_Image;  // 아이콘 이미지
    [SerializeField] private TextMeshProUGUI Amount_TMP;  // 수량 텍스트
    [SerializeField] private RectTransform   Icon_Visual; // 회전용 비주얼

    // ── 아이템 / 인벤토리 ─────────────────────────────────────────────────────
    public Item          ItemData        { get; private set; }
    public InventoryData OwnerData       { get; private set; }
    public UI_Inventory  ParentInventory { get; private set; }
    public int           SizeX = 1;
    public int           SizeY = 1;
    private float        CellSize;   // 마지막 Init에서 저장한 셀 크기
    private float        Spacing;    // 마지막 Init에서 저장한 간격

    // ── 장착 슬롯 ─────────────────────────────────────────────────────────────
    public UI_EquipSlot OwnerEquipSlot;
    private bool        originalIsRotated; // 장착 전 원본 회전 상태
    private bool        hasStoredOriginal; // 원본 상태 저장 여부

    // ── 드래그 ────────────────────────────────────────────────────────────────
    public static UI_Item_Icon      BeingDraggedIcon; // 현재 드래그 중인 아이콘
    [HideInInspector] public Transform startParent;   // 드롭 실패 시 복귀 부모
    private Vector3                 startPosition;    // 드롭 실패 시 복귀 위치
    private RectTransform           Rt;
    private UI_Inventory_Grid       LastHoveredGrid;  // 직전 프레임 호버 그리드
    private PointerEventData        LastEventData;    // Rotate 시 하이라이트 갱신용
    private GameObject              GhostIcon;        // 드래그 중 원위치 반투명 고스트


    // 아이템 데이터 초기화 — 외부에서 아이콘 생성 시 호출
    public void Init(Item item, float cellSize, float spacing, InventoryData ownerData = null, UI_Inventory parentInventory = null)
    {
        ItemData  = item;
        if (ownerData        != null) OwnerData       = ownerData;
        if (parentInventory  != null) ParentInventory = parentInventory;

        SizeX = ItemData.IsRotated ? ItemData.TileSizeY : ItemData.TileSizeX;
        SizeY = ItemData.IsRotated ? ItemData.TileSizeX : ItemData.TileSizeY;

        Rt = GetComponent<RectTransform>();
        CellSize = cellSize;
        Spacing  = spacing;

        float w = SizeX * cellSize + (SizeX - 1) * spacing;
        float h = SizeY * cellSize + (SizeY - 1) * spacing;
        Rt.sizeDelta = new Vector2(w, h);

        if (Icon_Visual != null)
        {
            // 회전 시 Icon_Visual 크기를 반전시켜 이미지가 올바르게 채워지도록
            Icon_Visual.sizeDelta     = ItemData.IsRotated ? new Vector2(h, w) : new Vector2(w, h);
            Icon_Visual.localRotation = Quaternion.Euler(0f, 0f, ItemData.IsRotated ? -90f : 0f);
        }

        if (ItemData.Icon != null)
            Icon_Image.sprite = ItemData.Icon;

        GetComponent<Image>().color = Mgr_ItemData.Inst.GetRarityColor(ItemData.Rarity);

        UpdateAmountText();
    }

    // 개수 텍스트 업데이트
    public void UpdateAmountText()
    {
        Amount_TMP.text = $"{ItemData.Amount}";
    }

    // 아이콘 90도 회전 — IsRotated 토글 후 Init 재호출로 크기/상태 갱신
    public void Rotate()
    {
        ItemData.ToggleRotation();


        if (ParentInventory != null) Init(ItemData, ParentInventory.Grid.CellSize, ParentInventory.Grid.Spacing);

        // 하이라이트 즉시 갱신
        if (LastEventData != null) OnDrag(LastEventData);
    }

    // 장착 슬롯 표시 여부에 따라 아이콘 크기 조정
    public void SetDisplaySize(bool isEquipSlot)
    {
        if (ItemData == null) return;

        if (isEquipSlot)
        {
            // 원본 회전 상태 저장 (처음 호출시만)
            if (!hasStoredOriginal)
            {
                originalIsRotated = ItemData.IsRotated;
                hasStoredOriginal = true;
            }

            // 2x2로 설정, 회전 해제
            SizeX = 2;
            SizeY = 2;
            if (ItemData.IsRotated) ItemData.ToggleRotation();
        }
        else
        {
            // 원본 회전 상태 복원
            if (ItemData.IsRotated != originalIsRotated) ItemData.ToggleRotation();

            // 원본 크기로 복원
            SizeX = ItemData.IsRotated ? ItemData.TileSizeY : ItemData.TileSizeX;
            SizeY = ItemData.IsRotated ? ItemData.TileSizeX : ItemData.TileSizeY;

            hasStoredOriginal = false;
        }

        // 저장된 CellSize/Spacing으로 RectTransform 직접 업데이트
        if (Rt == null) Rt = GetComponent<RectTransform>();
        float w = SizeX * CellSize + (SizeX - 1) * Spacing;
        float h = SizeY * CellSize + (SizeY - 1) * Spacing;
        Rt.sizeDelta = new Vector2(w, h);

        if (Icon_Visual != null)
        {
            Icon_Visual.sizeDelta     = ItemData.IsRotated ? new Vector2(h, w) : new Vector2(w, h);
            Icon_Visual.localRotation = Quaternion.Euler(0f, 0f, ItemData.IsRotated ? -90f : 0f);
        }
    }

    // 소속 인벤토리를 변경 — 이전 OwnerData에서 제거 후 새 OwnerData에 추가
    public void TransferTo(InventoryData targetData)
    {
        OwnerData?.Items.Remove(ItemData);
        OwnerData = targetData;
        OwnerData?.Items.Add(ItemData);
    }


    // 우클릭 시 컨텍스트 메뉴 표시
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        Mgr_UI.Inst.ShowContextMenu(this, eventData.position);
    }

    #region 드래그 관련

    // 마우스 버튼을 누른 채로 움직이기 시작하는 순간 한 번 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        BeingDraggedIcon = this;
        startPosition    = transform.position;
        startParent      = transform.parent;

        // 드롭 실패 대비 — 현재 슬롯에서 점유 해제
        ParentInventory?.Grid.Remove(ItemData.SlotX, ItemData.SlotY);

        // 원래 위치에 반투명 고스트 생성
        GhostIcon = Instantiate(gameObject, startParent);
        GhostIcon.GetComponent<CanvasGroup>().alpha         = 0.4f;
        GhostIcon.GetComponent<CanvasGroup>().blocksRaycasts = false;
        GhostIcon.GetComponent<CanvasGroup>().interactable   = false;

        GetComponent<CanvasGroup>().blocksRaycasts = false; // 드래그 중 자신의 레이캐스트 무시
        transform.SetParent(Mgr_UI.Inst.MainCanvas.transform);

        // 마우스 중앙으로 이동
        Rt.pivot = new Vector2(0.5f, 0.5f);

        // 장착 슬롯에서 드래그 시작 시 장착 해제
        OwnerEquipSlot?.Unequip();
        OwnerEquipSlot = null;
    }

    // IDragHandler — 드래그 중 매 프레임 호출
    public void OnDrag(PointerEventData eventData)
    {
        // 1. 마우스 위치로 아이콘 이동
        LastEventData = eventData;
        transform.position = eventData.position;

        // 2. 커서 아래 그리드(UI_Inventory_Grid) 탐색
        UI_Inventory_Grid hoveredGrid = GetGridUnderCursor(eventData);

        // 3. 그리드가 바뀌었으면 이전 그리드 하이라이트 초기화
        if (LastHoveredGrid != null && LastHoveredGrid != hoveredGrid)
            LastHoveredGrid.ResetHighlights();

        // 4. 그리드가 있으면 슬롯 탐색 후 하이라이트 상태 결정
        if (hoveredGrid != null)
        {
            hoveredGrid.ResetHighlights();
            UI_Item_Slot slot = GetSlotUnderCursor(eventData);
            if (slot != null)
            {
                UI_Item_Icon occupiedIcon = hoveredGrid.GetOccupiedIcon(slot.GridX, slot.GridY);
                EHighlightState state;

                if (occupiedIcon != null && ItemData.CanStackWith(occupiedIcon.ItemData))
                    state = EHighlightState.Valid;          // 완전 스택 가능
                else if (occupiedIcon != null && ItemData.CanPartialStack(occupiedIcon.ItemData))
                    state = EHighlightState.Partial;        // 부분 스택 가능
                else if (occupiedIcon != null && occupiedIcon.ItemData is Item_Bag bag && bag.BagInventory.CanAddItem(ItemData))
                    state = EHighlightState.Valid;          // 가방에 공간 있음
                else if (hoveredGrid.CanPlace(slot.GridX, slot.GridY, SizeX, SizeY))
                    state = EHighlightState.Valid;          // 빈 슬롯에 배치 가능
                else
                    state = EHighlightState.Invalid;        // 배치 불가

                hoveredGrid.HighlightSlots(slot.GridX, slot.GridY, SizeX, SizeY, state);
            }
        }

        LastHoveredGrid = hoveredGrid;
    }

    // IEndDragHandler — 마우스 버튼을 놓는 순간 한 번 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        BeingDraggedIcon = null;
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (LastHoveredGrid != null) LastHoveredGrid.ResetHighlights();
        LastHoveredGrid = null;

        // 고스트 제거
        if (GhostIcon != null) Destroy(GhostIcon);

        // OnDrop(슬롯/장착슬롯)이 OnEndDrag보다 먼저 호출됨
        // 드롭 성공 시 슬롯.OnDrop에서 부모가 변경됨
        // 부모가 여전히 MainCanvas = OnDrop 미호출 = 드롭 실패 → 원래 자리로 복귀
        if (transform.parent == Mgr_UI.Inst.MainCanvas.transform)
        {
            UI_Item_Slot targetSlot = GetSlotUnderCursor(eventData);
            if (targetSlot != null)
            {
                UI_Item_Icon occupiedIcon = targetSlot.Grid.GetOccupiedIcon(targetSlot.GridX, targetSlot.GridY);

                // 완전 스택
                if (occupiedIcon != null && ItemData.CanStackWith(occupiedIcon.ItemData))
                {
                    occupiedIcon.ItemData.AddAmount(ItemData.Amount);
                    occupiedIcon.UpdateAmountText();
                    OwnerData?.Items.Remove(ItemData);
                    Destroy(gameObject);
                    return;
                }

                // 부분 스택 — B를 MaxStack까지 채우고 A는 나머지 유지 후 원래 자리 복귀
                if (occupiedIcon != null && ItemData.CanPartialStack(occupiedIcon.ItemData))
                {
                    int canAdd = occupiedIcon.ItemData.MaxStack - occupiedIcon.ItemData.Amount;
                    occupiedIcon.ItemData.AddAmount(canAdd);
                    occupiedIcon.UpdateAmountText();
                    ItemData.DecreaseAmount(canAdd);
                    UpdateAmountText();
                }

                // 가방에 넣기 — 점유 아이콘이 가방이고 공간이 있으면 가방 인벤토리에 추가
                if (occupiedIcon != null && occupiedIcon.ItemData is Item_Bag bag && bag.BagInventory.CanAddItem(ItemData))
                {
                    bag.BagInventory.TryAddItem(ItemData);

                    // 가방UI 열려있으면 갱신
                    Mgr_UI.Inst.GetOpenInventory(bag.BagInventory)?.UpdateInventory(bag.BagInventory);

                    TransferTo(null);
                    Destroy(gameObject);
                    return;
                }
            }

            transform.position = startPosition;
            transform.SetParent(startParent);

            if (ParentInventory != null)
            {
                ParentInventory.Grid.Place(this, ItemData.SlotX, ItemData.SlotY);
            }
            else
            {
                UI_EquipSlot equipSlot = startParent.GetComponentInParent<UI_EquipSlot>();
                if (equipSlot != null)
                {
                    BeingDraggedIcon = this;
                    equipSlot.OnDrop(eventData);
                    BeingDraggedIcon = null;
                }
            }
        }
    }

    // 커서 아래 그리드 반환
    private UI_Inventory_Grid GetGridUnderCursor(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            UI_Inventory_Grid grid = result.gameObject.GetComponentInParent<UI_Inventory_Grid>();
            if (grid != null) return grid;
        }
        return null;
    }

    // 커서 아래 슬롯 반환
    private UI_Item_Slot GetSlotUnderCursor(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            UI_Item_Slot slot = result.gameObject.GetComponent<UI_Item_Slot>();
            if (slot != null) return slot;
        }
        return null;
    }

   

    #endregion
}
