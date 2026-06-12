using UnityEngine;

[System.Serializable]
public class Item_Bag : Item
{
    [field: SerializeField] public InventoryData BagInventory { get; private set; } // 가방 인벤토리 데이터
    public int           GridWidth    => BagInventory.GridWidth;  // 가방 가로 슬롯 수
    public int           GridHeight   => BagInventory.GridHeight; // 가방 세로 슬롯 수
    public Item_Bag(EItemType itemType, ERarity rarity, string id, string name, string description,
                    float weight, int maxStack, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                    int gridWidth, int gridHeight)
        : base(itemType, rarity, id, name, description, weight, maxStack, tileSizeX, tileSizeY, icon, prefab)
    {
        BagInventory = new InventoryData
        {
            GridWidth = gridWidth,
            GridHeight = gridHeight
        };
    }

    // BagInventory를 독립적으로 복사
    public override Item Copy()
    {
        Item_Bag copy = (Item_Bag)MemberwiseClone();
        copy.BagInventory = new InventoryData
        {
            GridWidth  = BagInventory.GridWidth,
            GridHeight = BagInventory.GridHeight
        };
        return copy;
    }

    // 가방 자체 무게 + 내부 아이템 총 무게 반환
    public float GetTotalWeight()
    {
        float total = Weight;
        foreach (Item item in BagInventory.Items)
            total += item.Weight * item.Amount;
        return total;
    }

    

    
}
