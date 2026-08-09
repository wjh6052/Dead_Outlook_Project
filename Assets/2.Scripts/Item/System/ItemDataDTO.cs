using System;
using System.Collections.Generic;

// JSON 저장용 아이템 데이터 — Sprite/GameObject 대신 키(string)로 저장
[Serializable]
public class ItemDataDTO
{
    public string ItemType;
    public string Rarity;
    public string ID;
    public string Name;
    public string Description;
    public float  Weight;
    public int    MaxStack;
    public int    TileSizeX;
    public int    TileSizeY;
    public string IconKey;
    public string PrefabKey;

    // Container 전용
    public string ContainerType;
    public int    GridWidth;
    public int    GridHeight;

    // Weapon 전용
    public string WeaponType;
    public string WeaponMeleeType;
    public float  Damage;
    public float  AttackSpeed;

    // Medical 전용
    public float HealAmount;
    public float UseTime;
}

[Serializable]
public class ItemDataList
{
    public List<ItemDataDTO> Items = new List<ItemDataDTO>();
}
