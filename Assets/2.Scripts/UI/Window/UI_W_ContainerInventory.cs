using UnityEngine;

public class UI_W_ContainerInventory : MonoBehaviour, IUI_WindowContent
{
    public UI_Window Window     { get; set; }
    public Vector2   WindowSize { get; private set; }

    private UI_ItemGrid ItemGrid;
    private Item_Container    Bag;

    private void OnDestroy()
    {
        if (Bag != null) Bag.OnMoved -= OnBagMoved;
    }

    // Ȯ������ ������ �ٸ� UI_ItemGrid�� �̵��ߴٸ� �ݱ�
    private void OnBagMoved() => Window.WindowClose();

    public void Init(Item_Container bag)
    {
        Bag = bag;
        Bag.OnMoved += OnBagMoved;

        InventoryData data = bag.ContainerInventory;
        float cell    = Mgr_UI.Inst.SlotCellSize;
        float spacing = Mgr_UI.Inst.SlotSpacing;
        WindowSize = new Vector2(
            data.GridWidth  * cell + (data.GridWidth  - 1) * spacing,
            data.GridHeight * cell + (data.GridHeight - 1) * spacing
        );

        ItemGrid = GetComponentInChildren<UI_ItemGrid>();
        ItemGrid.Init(data);
    }
}
