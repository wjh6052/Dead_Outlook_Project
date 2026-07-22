using UnityEngine;

public class Interactable_Item : Interactable
{
    public Item ItemData;

    public void Init(Item item, Vector3 pos)
    {
        ItemData = item;
        transform.position = pos;
        gameObject.layer = LayerMask.NameToLayer("Item");

        MeshCollider col = gameObject.AddComponent<MeshCollider>();
        col.sharedMesh   = GetComponent<MeshFilter>().sharedMesh;
        col.convex       = true;

        gameObject.AddComponent<Rigidbody>();
    }

    private void Start()
    {

    }



    public override void Interact()
    {
        if (ItemData == null) return;

        InventoryComponent inv = Mgr_PlayerController.Inst.Player.InventoryComp;
        if (inv.TryAddItem(ItemData))
        {
            Mgr_UI.Inst.ShowInteractUI(false);
            Destroy(gameObject);
        }
        // 가방 아이템이 인벤토리에 안 들어갈 경우 인벤토리 열고 가방 창 표시
        else if (ItemData is Item_Container bag)
        {
            Mgr_PlayerController.Inst.OnInventory();
            UI_W_ContainerInventory content = Instantiate(Resources.Load<UI_W_ContainerInventory>("Prefab/UI/Window/PF_UI_W_ContainerInventory"));
            content.Init(bag);
            Mgr_UI.Inst.SpawnWindow(bag.Name, content);
        }
    }
}
