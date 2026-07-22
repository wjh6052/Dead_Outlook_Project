using UnityEngine;

public class InventoryComponent : MonoBehaviour
{

    public InventoryData  PocketData; // 주머니 인벤토리 데이터 (고정)
    public Item_Container VestItem;   // 현재 장착된 조끼 아이템
    public Item_Container BagItem;    // 현재 장착된 가방 아이템

    // Pocket → Vest → Bag 순서로 아이템 추가 시도
    public bool TryAddItem(Item item)
    {
        if (PocketData.TryAddItem(item)) return true;
        if (VestItem?.ContainerInventory.TryAddItem(item) == true) return true;
        if (BagItem?.ContainerInventory.TryAddItem(item) == true) return true;
        return false;
    }

    private void Awake()
    {

    }

}
