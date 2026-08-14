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

    }

    public override void Interact()
    {
        Mgr_PlayerController.Inst.OnInventory();
        Mgr_UI.Inst.UI_Inventory_Player.ShowBox(BoxInventory);
    }
}
