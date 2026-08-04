using UnityEngine;

public class Interactable_Box : Interactable
{
    [SerializeField] private InventoryData BoxInventory; // 상자 인벤토리 데이터

    private void Start()
    {
        ItemSpawnEntry entry = GetComponent<ItemSpawnEntry>();
        if (entry == null) return;

        foreach (Item item in entry.GetItems())
            BoxInventory.TryAddItem(item);

        // 테스트 모든 아이템 추가
        foreach (Item item in Mgr_Item.Inst.GetAllItems())
            BoxInventory.TryAddItem(item.Copy());
    }

    public override void Interact()
    {
        Mgr_PlayerController.Inst.OnInventory();
        Mgr_UI.Inst.UI_Inventory_Player.ShowBox(BoxInventory);
    }
}
