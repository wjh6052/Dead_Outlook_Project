using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemGrid : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────────
    private RectTransform SlotLayer;
    private RectTransform IconLayer;

    // ── 데이터 ────────────────────────────────────────────────────────────────
    public InventoryData Data { get; private set; }

    // ── 그리드 ────────────────────────────────────────────────────────────────
    private GridLayoutGroup SlotGrid;
    protected UI_ItemSlot[,]  Slots;      // 좌표로 슬롯에 바로 접근 (하이라이트 등)
    protected UI_ItemIcon[,]  OccupiedBy; // 좌표별 점유 아이콘 (배치 가능 여부, 드래그 감지)


    private void OnDestroy()
    {
        if (Data != null) Data.OnChanged -= OnDataChanged;
    }

    private void OnDataChanged() => Init(Data);

    public void Init(InventoryData data)
    {
        if (Data != null) Data.OnChanged -= OnDataChanged;
        if (SlotGrid == null)
        {
            SlotLayer = transform.Find("SlotLayer").GetComponent<RectTransform>();
            SlotGrid  = SlotLayer.GetComponent<GridLayoutGroup>();
            SlotGrid.cellSize = Vector2.one * Mgr_UI.Inst.SlotCellSize;
            SlotGrid.spacing  = Vector2.one * Mgr_UI.Inst.SlotSpacing;
        }
        if (IconLayer == null) IconLayer = transform.Find("IconLayer").GetComponent<RectTransform>();

        // 기존 슬롯/아이콘 제거
        foreach (Transform child in SlotLayer) Destroy(child.gameObject);
        foreach (Transform child in IconLayer) Destroy(child.gameObject);

        if (data == null) { Data = null; Slots = null; return; }

        Data = data;
        Data.OnChanged += OnDataChanged;

        // 슬롯 생성
        Slots = new UI_ItemSlot[Data.GridWidth, Data.GridHeight];
        SlotGrid.constraintCount = Data.GridWidth;

        for (int y = 0; y < Data.GridHeight; y++)
            for (int x = 0; x < Data.GridWidth; x++)
            {
                UI_ItemSlot slot = Instantiate(Mgr_UI.Inst.SlotPrefab, SlotLayer);
                slot.Init(this, x, y);
                Slots[x, y] = slot;
            }

        // 아이콘 재배치
        OccupiedBy = new UI_ItemIcon[Data.GridWidth, Data.GridHeight];
        List<Item> itemsToPlace = new List<Item>(Data.Items);
        Data.Items.Clear();
        foreach (Item item in itemsToPlace) PlaceItemAt(item, item.SlotX, item.SlotY);
    }

    
    // 아이템을 빈 자리에 추가 — 실패 시 false 반환
    public bool TryAddItem(Item item)
    {
        if (!TryFindEmptyPos(item, out int x, out int y, out bool needsRotation)) return false;

        if (needsRotation) item.ToggleRotation();

        item.SetSlot(x, y);
        Data.Items.Add(item);

        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        // 아이콘 생성
        UI_ItemIcon icon = Instantiate(Mgr_UI.Inst.IconPrefab, IconLayer);
        icon.Init(item, this);

        // OccupiedBy 등록
        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
                OccupiedBy[x + dx, y + dy] = icon;

        return true;
    }

    #region UI_ItemIcon에서 호출
    // (x, y) 기준으로 아이템 크기만큼 슬롯 하이라이트
    public virtual void HighlightSlots(int x, int y, Item item)
    {
        ClearHighlights();

        // 해당 좌표에 다른 아이콘이 있으면 가방/스택 가능 여부 먼저 확인
        UI_ItemIcon targetIcon = OccupiedBy[x, y];
        if (targetIcon != null && targetIcon != UI_ItemIcon.BeingDragged)
        {
            // 가방이면 내부 공간 확인
            if (targetIcon.ItemData is Item_Container bag)
            {
                HighlightIconSlots(targetIcon, bag.ContainerInventory.CanAddItem(item) ? ESlotHighlight.Valid : ESlotHighlight.Invalid);
                return;
            }

            ESlotHighlight stackHighlight;
            if (item.CanStackWith(targetIcon.ItemData)) stackHighlight = ESlotHighlight.Valid;
            else if (item.CanPartialStack(targetIcon.ItemData)) stackHighlight = ESlotHighlight.Partial;
            else stackHighlight = ESlotHighlight.Invalid;
            HighlightIconSlots(targetIcon, stackHighlight);
            return;
        }

        // 빈 슬롯 → CanPlaceAt으로 배치 가능 여부 확인
        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;
        int anchorX = x - sizeX / 2;
        int anchorY = y - sizeY / 2;

        ESlotHighlight highlight = CanPlaceAt(anchorX, anchorY, item, UI_ItemIcon.BeingDragged) ? ESlotHighlight.Valid : ESlotHighlight.Invalid;

        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
            {
                int nx = anchorX + dx, ny = anchorY + dy;
                if (nx >= 0 && nx < Data.GridWidth && ny >= 0 && ny < Data.GridHeight)
                    Slots[nx, ny].SetHighlight(highlight);
            }
    }

    // 모든 슬롯 하이라이트 초기화
    public void ClearHighlights()
    {
        foreach (UI_ItemSlot slot in Slots)
            slot.SetHighlight(ESlotHighlight.Default);
    }

    // 아이콘이 점유한 슬롯 전체에 하이라이트 적용
    private void HighlightIconSlots(UI_ItemIcon icon, ESlotHighlight highlight)
    {
        int tx    = icon.ItemData.SlotX;
        int ty    = icon.ItemData.SlotY;
        int tsizX = icon.ItemData.IsRotated ? icon.ItemData.TileSizeY : icon.ItemData.TileSizeX;
        int tsizY = icon.ItemData.IsRotated ? icon.ItemData.TileSizeX : icon.ItemData.TileSizeY;
        for (int dy = 0; dy < tsizY; dy++)
            for (int dx = 0; dx < tsizX; dx++)
                Slots[tx + dx, ty + dy].SetHighlight(highlight);
    }

    // 드롭 처리 — 스택 체크 후 배치, 성공 시 true / 복귀 필요 시 false
    public virtual bool TryDrop(Item item, int x, int y, UI_ItemGrid source, UI_ItemIcon sourceIcon)
    {
        UI_ItemIcon targetIcon = OccupiedBy[x, y];

        // 스택 체크
        if (targetIcon != null && targetIcon != sourceIcon)
        {
            // 가방이면 내부로 아이템 이동
            if (targetIcon.ItemData is Item_Container bag && bag.ContainerInventory.TryAddItem(item))
            {
                source.RemoveItem(sourceIcon);
                Destroy(sourceIcon.gameObject);
                return true;
            }

            // 완전 스택
            if (item.CanStackWith(targetIcon.ItemData))
            {
                targetIcon.ItemData.AddAmount(item.Amount);
                targetIcon.UpdateAmount();
                source.RemoveItem(sourceIcon);
                Destroy(sourceIcon.gameObject);
                return true;
            }

            // 부분 스택 — 대상을 MaxStack까지 채우고 복귀
            if (item.CanPartialStack(targetIcon.ItemData))
            {
                int space = targetIcon.ItemData.MaxStack - targetIcon.ItemData.Amount;
                targetIcon.ItemData.AddAmount(space);
                targetIcon.UpdateAmount();
                item.DecreaseAmount(space);
                sourceIcon.UpdateAmount();
                return false;
            }
        }

        // 일반 배치
        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;
        int anchorX = x - sizeX / 2;
        int anchorY = y - sizeY / 2;

        if (CanPlaceAt(anchorX, anchorY, item, sourceIcon))
        {
            source.RemoveItem(sourceIcon);

            item.SetSlot(anchorX, anchorY);
            Data.Items.Add(item);

            int sizeXFull = item.IsRotated ? item.TileSizeY : item.TileSizeX;
            int sizeYFull = item.IsRotated ? item.TileSizeX : item.TileSizeY;
            for (int dy = 0; dy < sizeYFull; dy++)
                for (int dx = 0; dx < sizeXFull; dx++)
                    OccupiedBy[anchorX + dx, anchorY + dy] = sourceIcon;

            sourceIcon.MoveTo(this, IconLayer);
            return true;
        }

        return false;
    }
    #endregion




    // (0,0)부터 탐색해 아이템이 들어갈 수 있는 첫 번째 위치를 찾아 반환
    // 현재 회전으로 실패 시 회전 후 재탐색 — 회전이 필요하면 needsRotation = true
    private bool TryFindEmptyPos(Item item, out int posX, out int posY, out bool needsRotation)
    {
        needsRotation = false;

        if (TryFindEmptyPosWithRotation(item, out posX, out posY)) return true;

        // 회전 후 재시도
        item.ToggleRotation();
        if (TryFindEmptyPosWithRotation(item, out posX, out posY))
        {
            needsRotation = true;
            item.ToggleRotation(); // 실제 적용은 TryAddItem에서 처리
            return true;
        }

        item.ToggleRotation(); // 복구
        posX = posY = -1;
        return false;
    }

    // 현재 회전 상태 그대로 아이템이 들어갈 수 있는 첫 번째 빈 위치를 탐색 — 없으면 false 반환
    private bool TryFindEmptyPosWithRotation(Item item, out int posX, out int posY)
    {
        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        for (int y = 0; y <= Data.GridHeight - sizeY; y++)
            for (int x = 0; x <= Data.GridWidth - sizeX; x++)
            {
                bool fits = true;
                for (int dy = 0; dy < sizeY && fits; dy++)
                    for (int dx = 0; dx < sizeX && fits; dx++)
                        if (OccupiedBy[x + dx, y + dy] != null) fits = false;

                if (fits) { posX = x; posY = y; return true; }
            }

        posX = posY = -1;
        return false;
    }

    // (x, y) 위치에 item을 배치할 수 있는지 확인 — exclude 아이콘은 점유 체크에서 제외
    public bool CanPlaceAt(int x, int y, Item item, UI_ItemIcon exclude = null)
    {
        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        if (x < 0 || y < 0 || x + sizeX > Data.GridWidth || y + sizeY > Data.GridHeight) return false;

        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
            {
                UI_ItemIcon occupant = OccupiedBy[x + dx, y + dy];
                if (occupant != null && occupant != exclude) return false;
            }

        return true;
    }

    // 인벤토리에서 아이템 데이터 제거 + OccupiedBy 초기화
    public virtual void RemoveItem(UI_ItemIcon icon)
    {
        Data.Items.Remove(icon.ItemData);
        for (int y = 0; y < Data.GridHeight; y++)
            for (int x = 0; x < Data.GridWidth; x++)
                if (OccupiedBy[x, y] == icon) OccupiedBy[x, y] = null;
        icon.ItemData.OnMoved?.Invoke();
    }

    // 특정 좌표에 아이템 배치 + 아이콘 생성 (드롭 성공 시 호출)
    public void PlaceItemAt(Item item, int x, int y)
    {
        item.SetSlot(x, y);
        Data.Items.Add(item);

        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        // 아이콘 생성 및 OccupiedBy 등록
        UI_ItemIcon icon = Instantiate(Mgr_UI.Inst.IconPrefab, IconLayer);
        icon.Init(item, this);

        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
                OccupiedBy[x + dx, y + dy] = icon;
    }

}
