using UnityEngine;

// 은신처 보관함 상호작용
// - 상호작용 시 Mgr_Hideout의 StorageData를 인벤토리 UI에 표시
public class Interactable_Storage : Interactable
{
    public override void Interact()
    {
        Mgr_PlayerController.Inst.OnInventory();
        Mgr_UI.Inst.UI_Inventory_Player.ShowBox(Mgr_Hideout.Inst.StorageData);
    }
}
