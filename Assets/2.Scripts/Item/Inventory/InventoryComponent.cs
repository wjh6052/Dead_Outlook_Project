using UnityEngine;

public class InventoryComponent : MonoBehaviour
{

    public InventoryData PocketData; // 주머니 인벤토리 데이터 (고정)
    public Item_Bag      BeltItem;   // 현재 장착된 벨트 아이템
    public Item_Bag      BagItem;    // 현재 장착된 가방 아이템


    private void Awake()
    {

    }

}
