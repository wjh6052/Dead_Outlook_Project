using UnityEngine;

public class UI_Inventory : MonoBehaviour
{
    InventoryComponent Player_Inventory;

    [Header("EquipSlot")]
    [SerializeField] private UI_EquipSlot Vest_EquipSlot;
    [SerializeField] private UI_EquipSlot Bag_EquipSlot;
    [SerializeField] private UI_EquipSlot Weapon_1_EquipSlot;
    [SerializeField] private UI_EquipSlot Weapon_2_EquipSlot;


    [Header("ItemGrid")]
    [SerializeField] private UI_ItemGrid Pocket_ItemGrid;
    [SerializeField] private UI_ItemGrid Vest_ItemGrid;
    [SerializeField] private UI_ItemGrid Bag_ItemGrid;

    [Header("Box_ItemGrid")]
    [SerializeField] private UI_ItemGrid Box_ItemGrid;


    // Mgr_UI.Start에서 호출 — 비활성화 상태에서도 초기화 가능
    public void Setup()
    {
        Player_Inventory = Character_Player.Inst.InventoryComp;

        Vest_EquipSlot.Setup();
        Bag_EquipSlot.Setup();
        Weapon_1_EquipSlot.Setup();
        Weapon_2_EquipSlot.Setup();

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
        Weapon_1_EquipSlot.Init(item => Player_Inventory.SetWeapon(true,  item));
        Weapon_2_EquipSlot.Init(item => Player_Inventory.SetWeapon(false, item));
    }

    public void Init()
    {
        Pocket_ItemGrid.Init(Player_Inventory.PocketData);

        Vest_ItemGrid.gameObject.SetActive(Player_Inventory.VestItem != null);
        Vest_ItemGrid.Init(Player_Inventory.VestItem?.ContainerInventory);
        Bag_ItemGrid.gameObject.SetActive(Player_Inventory.BagItem != null);
        Bag_ItemGrid.Init(Player_Inventory.BagItem?.ContainerInventory);

        ShowBox();
    }

    // 아이템 타입에 맞는 장착 슬롯 확인 — 빈 슬롯이 있으면 장착 후 true, 없으면 false
    public bool TryEquip(Item item)
    {
        if (item is Item_Container container)
        {
            switch (container.ContainerType)
            {
                case EContainerType.Vest:
                    if (Vest_EquipSlot.EquippedIcon != null) return false;
                    Vest_EquipSlot.Equip(item);
                    return true;
                case EContainerType.Bag:
                    if (Bag_EquipSlot.EquippedIcon != null) return false;
                    Bag_EquipSlot.Equip(item);
                    return true;
            }
            return false;
        }

        if (item.ItemType == EItemType.WeaponMelee || item.ItemType == EItemType.WeaponRanged)
        {
            if (Weapon_1_EquipSlot.EquippedIcon == null) { Weapon_1_EquipSlot.Equip(item); return true; }
            if (Weapon_2_EquipSlot.EquippedIcon == null) { Weapon_2_EquipSlot.Equip(item); return true; }
            return false;
        }

        return false;
    }

    // 상자 인벤토리 표시 — null이면 패널 숨김, 데이터가 있으면 해당 인벤토리 표시
    public void ShowBox(InventoryData data = null)
    {
        Box_ItemGrid.gameObject.SetActive(data != null);
        if (data != null) Box_ItemGrid.Init(data);
    }


}
