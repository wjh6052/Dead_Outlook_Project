using UnityEngine;

public enum EWeaponType
{
    Melee,  // 근접무기
    Ranged, // 원거리무기
}

[System.Serializable]
public class Item_Weapon : Item
{
    [field: SerializeField] public EWeaponType WeaponType  { get; private set; } // 무기 종류 (근접/원거리)
    [field: SerializeField] public float       Damage      { get; private set; } // 공격 데미지
    [field: SerializeField] public float       AttackSpeed { get; private set; } // 공격 쿨타임 (초)

    public Item_Weapon(EItemType itemType, ERarity rarity, string id, string name, string description,
                       float weight, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                       EWeaponType weaponType, float damage, float attackSpeed)
        : base(itemType, rarity, id, name, description, weight, 1, tileSizeX, tileSizeY, icon, prefab)
    {
        WeaponType  = weaponType;
        Damage      = damage;
        AttackSpeed = attackSpeed;
    }
}
