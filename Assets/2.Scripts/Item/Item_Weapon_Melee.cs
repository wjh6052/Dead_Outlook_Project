using UnityEngine;

public enum EWeaponMeleeType
{
    Fist = 0, // 주먹
    Bat  = 1, // 배트
    Axe  = 2, // 도끼
}

[System.Serializable]
public class Item_Weapon_Melee : Item_Weapon
{
    [field: SerializeField] public EWeaponMeleeType WeaponMeleeType { get; private set; } // 무기 종류

    public Item_Weapon_Melee(ERarity rarity, string id, string name, string description,
                             float weight, int tileSizeX, int tileSizeY, Sprite icon, GameObject prefab,
                             EWeaponType weaponType, EWeaponMeleeType weaponMeleeType, float damage, float attackSpeed)
        : base(EItemType.WeaponMelee, rarity, id, name, description, weight, tileSizeX, tileSizeY, icon, prefab,
               weaponType, damage, attackSpeed)
    {
        WeaponMeleeType = weaponMeleeType;
    }
}
