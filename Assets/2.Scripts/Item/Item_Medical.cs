using UnityEngine;

[System.Serializable]
public class Item_Medical : Item
{
    [field: SerializeField] public float HealAmount { get; private set; } // 회복량
    [field: SerializeField] public float UseTime    { get; private set; } // 사용 시간 (초)

    public Item_Medical(ERarity rarity, string id, string name, string description,
                        float weight, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                        float healAmount, float useTime)
        : base(EItemType.Medical, rarity, id, name, description, weight, 10, tileSizeX, tileSizeY, icon, prefab)
    {
        HealAmount = healAmount;
        UseTime    = useTime;
    }
}
