using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    InventoryComponent Player_Inventory;

    [Header("EquipSlot")]
    [SerializeField] private UI_EquipSlot Bag_EquipSlot;


    [Header("ItemGrid")]
    [SerializeField] private UI_ItemGrid Pocket_ItemGrid;
    [SerializeField] private UI_ItemGrid Bag_ItemGrid;


    

    public void Init()
    {
        if (Player_Inventory == null)
        {
            Player_Inventory = Mgr_PlayerController.Inst.Player.InventoryComp;
        }


        Bag_EquipSlot.Init(item =>
        {
            Player_Inventory.BagItem = item as Item_Bag;
            Bag_ItemGrid.gameObject.SetActive(Player_Inventory.BagItem != null);
            Bag_ItemGrid.Init(Player_Inventory.BagItem?.BagInventory);
        });

        Pocket_ItemGrid.Init(Player_Inventory.PocketData);
        Bag_ItemGrid.gameObject.SetActive(Player_Inventory.BagItem != null);
        Bag_ItemGrid.Init(Player_Inventory.BagItem?.BagInventory);

    }


    // Å×½ºÆ®
    [Header("Test")]
    [SerializeField] private InventoryData data;
    [SerializeField] private UI_ItemGrid Test_ItemGrid;
    void Start()
    {
        Test_ItemGrid.Init(data);

        Item i = Mgr_ItemData.Inst.GetItem("Test_1");
        Test_ItemGrid.TryAddItem(i.Copy());
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_1"));
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_2"));
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_3"));
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Belt_1"));
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Bag_2"));
        Test_ItemGrid.TryAddItem(Mgr_ItemData.Inst.GetItem("Test_Bag_3"));
    }

    void Update()
    {
        
    }
}
