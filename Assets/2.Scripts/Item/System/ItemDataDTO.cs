using System;
using System.Collections.Generic;

// JSON 저장용 아이템 데이터 — Sprite/GameObject 대신 키(string)로 저장
// 공통 칸만 가짐 (전용 칸이 없는 General · Armor · Tool은 이 클래스 그대로 사용)
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
}

// Container 전용
[Serializable]
public class ItemDataDTO_Container : ItemDataDTO
{
    public string ContainerType;
    public int    GridWidth;
    public int    GridHeight;
}

// Weapon 전용
[Serializable]
public class ItemDataDTO_Weapon : ItemDataDTO
{
    public string WeaponType;
    public string WeaponMeleeType;
    public float  Damage;
    public float  AttackSpeed;
}

// Medical 전용
[Serializable]
public class ItemDataDTO_Medical : ItemDataDTO
{
    public float  HealAmount;
    public float  UseTime;
    public string MedicalType;  // EMedicalType 이름 (Bandage)
}

// Food 전용
[Serializable]
public class ItemDataDTO_Food : ItemDataDTO
{
    public float  HungerAmount;
    public float  ThirstAmount;
    public float  UseTime;
    public string EatType;      // EEatType 이름 (Eat · Drink)
}

// 파일 하나(ItemData_{종류}.json)의 아이템 목록 — JsonUtility는 목록만 바로 저장하지 못해 한 번 감쌈
[Serializable]
public class ItemDataList<T> where T : ItemDataDTO
{
    public List<T> Items = new List<T>();
}
