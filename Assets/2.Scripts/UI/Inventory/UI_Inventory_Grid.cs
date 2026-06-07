using UnityEngine;
using UnityEngine.UI;

public enum EHighlightState { Valid, Invalid, Partial }

public class UI_Inventory_Grid : MonoBehaviour
{
    [SerializeField] private GameObject SlotPrefab; // 슬롯 프리팹

    int Width;
    int Height;

    public int GetWidth()  => Width;
    public int GetHeight() => Height;

    public UI_Inventory Inventory { get; private set; } // 부모 인벤토리 참조
    public float        CellSize  { get; private set; } // 셀 크기
    public float        Spacing   { get; private set; } // 셀 간격

    GridLayoutGroup GridLayout;

    public  UI_Item_Slot[,] Slots;            // 슬롯 2D 배열
    private UI_Item_Icon[,] OccupiedBy;       // 각 셀을 점유한 아이콘

    private void Awake()
    {
        Inventory  = GetComponentInParent<UI_Inventory>();
        GridLayout = GetComponent<GridLayoutGroup>();
    }

    // InventoryData를 받아 그리드 초기화 — UI_Inventory.UpdateInventory에서 호출
    public void Init(InventoryData data)
    {
        // GridLayoutGroup에서 최신 값 읽기
        CellSize = GridLayout.cellSize.x;
        Spacing  = GridLayout.spacing.x;

        // 기존 자식 오브젝트 전부 제거
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Width  = data.GridWidth;
        Height = data.GridHeight;

        Slots      = new UI_Item_Slot[Width, Height];
        OccupiedBy = new UI_Item_Icon[Width, Height];

        GridLayout.constraintCount = Width;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                GameObject   obj  = Instantiate(SlotPrefab, transform);
                UI_Item_Slot slot  = obj.GetComponent<UI_Item_Slot>();
                slot.Init(x, y, this);
                Slots[x, y] = slot;
            }
        }
    }

    // 해당 범위 슬롯들을 아이콘으로 점유
    public void Place(UI_Item_Icon icon, int x, int y)
    {
        for (int dy = 0; dy < icon.SizeY; dy++)
            for (int dx = 0; dx < icon.SizeX; dx++)
                OccupiedBy[x + dx, y + dy] = icon;

        float cellSize = CellSize;
        float spacing  = Spacing;

        // 아이콘을 IconLayer 자식으로 배치 — 슬롯 위에 렌더링
        icon.transform.SetParent(Inventory.IconLayer);
        if (icon.ItemData != null)
        {
            icon.ItemData.SetSlot(x, y);
            icon.Init(icon.ItemData, cellSize, spacing, null, Inventory);
        }

        // pivot/anchor (0,1) 기준 — 그리드 좌상단에서 셀 좌표로 위치 계산
        RectTransform iconRt = icon.GetComponent<RectTransform>();
        iconRt.pivot            = new Vector2(0f, 1f);
        iconRt.anchorMin        = new Vector2(0f, 1f);
        iconRt.anchorMax        = new Vector2(0f, 1f);
        iconRt.anchoredPosition = new Vector2(
            GridLayout.padding.left + x * (cellSize + spacing),
           -(GridLayout.padding.top  + y * (cellSize + spacing))
        );
    }

    // 해당 범위가 모두 비어있는지 확인
    public bool CanPlace(int x, int y, int sizeX, int sizeY)
    {
        if (x < 0 || y < 0 || x + sizeX > Width || y + sizeY > Height) return false;

        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
                if (OccupiedBy[x + dx, y + dy] != null) return false;

        return true;
    }
  

    // 해당 좌표 아이콘이 차지한 슬롯 전체 해제
    public void Remove(int x, int y)
    {
        UI_Item_Icon icon = OccupiedBy[x, y];
        if (icon == null) return;

        for (int dy = 0; dy < Height; dy++)
            for (int dx = 0; dx < Width; dx++)
                if (OccupiedBy[dx, dy] == icon) OccupiedBy[dx, dy] = null;
    }

    // 해당 범위 슬롯 하이라이트 — Valid: 초록, Invalid: 빨강, Partial: 노랑
    public void HighlightSlots(int x, int y, int sizeX, int sizeY, EHighlightState state)
    {
        for (int dy = 0; dy < sizeY; dy++)
            for (int dx = 0; dx < sizeX; dx++)
                if (x + dx < Width && y + dy < Height)
                {
                    var slot = Slots[x + dx, y + dy];
                    switch (state)
                    {
                        case EHighlightState.Valid:   slot.SetHighlight(); break;
                        case EHighlightState.Invalid: slot.SetInvalid();   break;
                        case EHighlightState.Partial: slot.SetPartial();   break;
                    }
                }
    }

    // 전체 슬롯 색상 초기화
    public void ResetHighlights()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                Slots[x, y].ResetColor();
    }

    // 해당 좌표를 점유한 아이콘 반환
    public UI_Item_Icon GetOccupiedIcon(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        return OccupiedBy[x, y];
    }

    // 좌표로 슬롯 반환
    public UI_Item_Slot GetSlot(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;
        return Slots[x, y];
    }
}


