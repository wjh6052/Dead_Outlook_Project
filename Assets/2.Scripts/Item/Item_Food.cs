using UnityEngine;

// 음식 먹는 애니메이션 종류
public enum EEatType
{
    Eat,   // 먹기
    Drink, // 마시기
}

// 음식 아이템 — 음료도 포함 (배고픔 · 목마름 회복량으로 구분)
[System.Serializable]
public class Item_Food : Item
{
    [field: SerializeField] public float    HungerAmount { get; private set; } // 배고픔 회복량
    [field: SerializeField] public float    ThirstAmount { get; private set; } // 목마름 회복량
    [field: SerializeField] public float    UseTime      { get; private set; } // 먹는 시간 (초)
    [field: SerializeField] public EEatType EatType      { get; private set; } // 먹는 애니메이션 종류

    public Item_Food(ERarity rarity, string id, string name, string description,
                     float weight, int maxStack, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                     float hungerAmount, float thirstAmount, float useTime, EEatType eatType)
        : base(EItemType.Food, rarity, id, name, description, weight, maxStack, tileSizeX, tileSizeY, icon, prefab)
    {
        HungerAmount = hungerAmount;
        ThirstAmount = thirstAmount;
        UseTime      = useTime;
        EatType      = eatType;
    }
}
