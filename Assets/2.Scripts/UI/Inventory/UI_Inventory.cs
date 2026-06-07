using UnityEngine;
using UnityEngine.InputSystem; // 테스트용 삭제필요

public class UI_Inventory : MonoBehaviour
{
    [SerializeField] public RectTransform IconLayer;          // 아이콘 전용 레이어 (슬롯 위에 렌더링)
    
    
    public UI_Inventory_Grid Grid      { get; private set; } // 자식 그리드 참조
    public InventoryData     Data      { get; private set; } // 현재 연결된 인벤토리 데이터
    private GameObject IconPrefab;                            // 아이콘 프리팹



    private void Awake()
    {
        Grid = GetComponentInChildren<UI_Inventory_Grid>();
        IconPrefab = Resources.Load<GameObject>("Prefab/UI/Inventory/PF_UI_Item_Icon");
    }


    // InventoryData를 받아 그리드 초기화 후 아이템 배치
    public void UpdateInventory(InventoryData data)
    {
        // 데이터의 주소값을 저장
        Data = data;
        Grid.Init(data);

        // IconLayer 기존 아이콘 전부 제거
        foreach (Transform child in IconLayer)
            Destroy(child.gameObject);

        foreach (Item item in data.Items)
            PlaceAt(item, item.SlotX, item.SlotY);
    }

    // 저장된 위치에 아이콘 생성 및 배치 — UpdateInventory에서 호출
    private void PlaceAt(Item item, int x, int y)
    {
        GameObject   obj  = Instantiate(IconPrefab, IconLayer);
        UI_Item_Icon icon = obj.GetComponent<UI_Item_Icon>();
        icon.Init(item, Grid.CellSize, Grid.Spacing, Data, this);
        Grid.Place(icon, x, y);
    }

    // 0,0부터 탐색해서 아이템을 넣을 수 있는 첫 번째 위치에 배치 — 실패 시 회전 후 재시도, 그래도 없으면 false 반환
    public bool TryAddItem(Item item, bool isUpdate = false)
    {
        if (TryPlace(item, isUpdate)) return true;

        // 회전해서 재시도 (정사각형 아이템은 불필요)
        if (item.TileSizeX != item.TileSizeY)
        {
            item.ToggleRotation();
            if (TryPlace(item, isUpdate)) return true;
            item.ToggleRotation(); // 실패 시 원래대로 복원
        }

        Debug.LogWarning($"[UI_Inventory] 공간 부족 — {item.Name} ({item.ID}) 배치 실패");
        return false;
    }

    private bool TryPlace(Item item, bool isUpdate)
    {
        // IsRotated 상태 기준으로 실제 크기 계산 — CanPlace와 Place 간 불일치 방지
        int sizeX = item.IsRotated ? item.TileSizeY : item.TileSizeX;
        int sizeY = item.IsRotated ? item.TileSizeX : item.TileSizeY;

        for (int y = 0; y < Grid.GetHeight(); y++)
        {
            for (int x = 0; x < Grid.GetWidth(); x++)
            {
                if (!Grid.CanPlace(x, y, sizeX, sizeY)) continue;

                GameObject   obj  = Instantiate(IconPrefab, IconLayer);
                UI_Item_Icon icon = obj.GetComponent<UI_Item_Icon>();
                icon.Init(item, Grid.CellSize, Grid.Spacing, Data, this);
                item.SetSlot(x, y);
                Grid.Place(icon, x, y);
                if (!isUpdate && Data != null) Data.Items.Add(item);
                return true;
            }
        }

        return false;
    }
}
