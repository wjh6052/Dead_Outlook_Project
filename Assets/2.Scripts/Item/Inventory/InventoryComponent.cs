using UnityEngine;

public class InventoryComponent : MonoBehaviour
{
    [SerializeField] private int PocketWidth  = 4; // 주머니 가로 슬롯 수
    [SerializeField] private int PocketHeight = 1; // 주머니 세로 슬롯 수

    public Item_Bag      BeltItem;   // 현재 장착된 벨트 아이템
    public Item_Bag      BagItem;    // 현재 장착된 가방 아이템
    public InventoryData PocketData; // 주머니 인벤토리 데이터 (고정)

    public InventoryData Inventory_Test; // 주머니 인벤토리 데이터 (고정)
    private void Awake()
    {
        PocketData = new InventoryData { GridWidth = PocketWidth, GridHeight = PocketHeight };
    }

}
