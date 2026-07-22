using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    InventoryComponent Player_Inventory;

    [Header("EquipSlot")]
    [SerializeField] private UI_EquipSlot Vest_EquipSlot;
    [SerializeField] private UI_EquipSlot Bag_EquipSlot;


    [Header("ItemGrid")]
    [SerializeField] private UI_ItemGrid Pocket_ItemGrid;
    [SerializeField] private UI_ItemGrid Vest_ItemGrid;
    [SerializeField] private UI_ItemGrid Bag_ItemGrid;

    [Header("Box_ItemGrid")]
    [SerializeField] private UI_ItemGrid Box_ItemGrid;


    private void Awake()
    {
        Player_Inventory = Mgr_PlayerController.Inst.Player.InventoryComp;
    }

    public void Init()
    {
        Vest_EquipSlot.Init(item =>
        {
            Player_Inventory.VestItem = item as Item_Container;
            Vest_ItemGrid.gameObject.SetActive(Player_Inventory.VestItem != null);
            Vest_ItemGrid.Init(Player_Inventory.VestItem?.ContainerInventory);
        });

        Bag_EquipSlot.Init(item =>
        {
            Player_Inventory.BagItem = item as Item_Container;
            Bag_ItemGrid.gameObject.SetActive(Player_Inventory.BagItem != null);
            Bag_ItemGrid.Init(Player_Inventory.BagItem?.ContainerInventory);
        });

        Pocket_ItemGrid.Init(Player_Inventory.PocketData);
        Vest_ItemGrid.gameObject.SetActive(Player_Inventory.VestItem != null);
        Vest_ItemGrid.Init(Player_Inventory.VestItem?.ContainerInventory);
        Bag_ItemGrid.gameObject.SetActive(Player_Inventory.BagItem != null);
        Bag_ItemGrid.Init(Player_Inventory.BagItem?.ContainerInventory);

        // 상자 인벤토리 패널 숨김 기본
        ShowBox();
    }

    // 상자 인벤토리 표시 — null이면 패널 숨김, 데이터가 있으면 해당 인벤토리 표시
    public void ShowBox(InventoryData data = null)
    {
        Box_ItemGrid.gameObject.SetActive(data != null);
        if (data != null) Box_ItemGrid.Init(data);
    }


}
