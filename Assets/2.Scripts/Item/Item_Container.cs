using UnityEngine;

public enum EContainerType // 컨테이너 서브타입 열거형
{
    Bag,    // 가방
    Vest,   // 조끼
    Case,   // 케이스
}

[System.Serializable]
public class Item_Container : Item
{
    [field: SerializeField] public EContainerType  ContainerType      { get; private set; } // 컨테이너 서브타입
    [field: SerializeField] public InventoryData   ContainerInventory { get; private set; } // 내부 인벤토리 데이터
    public int GridWidth  => ContainerInventory.GridWidth;  // 가로 슬롯 수
    public int GridHeight => ContainerInventory.GridHeight; // 세로 슬롯 수

    public Item_Container(EItemType itemType, ERarity rarity, string id, string name, string description,
                          float weight, int maxStack, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                          EContainerType containerType, int gridWidth, int gridHeight)
        : base(itemType, rarity, id, name, description, weight, maxStack, tileSizeX, tileSizeY, icon, prefab)
    {
        ContainerType      = containerType;
        ContainerInventory = new InventoryData
        {
            GridWidth  = gridWidth,
            GridHeight = gridHeight
        };
    }

    // ContainerInventory를 독립적으로 복사
    public override Item Copy()
    {
        Item_Container copy = (Item_Container)MemberwiseClone();
        copy.ContainerInventory = new InventoryData
        {
            GridWidth  = ContainerInventory.GridWidth,
            GridHeight = ContainerInventory.GridHeight
        };
        return copy;
    }

    // 가방 자체 무게 + 내부 아이템 총 무게 반환
    public float GetTotalWeight()
    {
        float total = Weight;
        foreach (Item item in ContainerInventory.Items)
            total += item.Weight * item.Amount;
        return total;
    }

    

    
}
