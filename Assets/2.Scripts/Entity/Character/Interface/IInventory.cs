public interface IInventory
{
    int           CurrentWeaponSlot { get; } // 현재 선택된 무기 슬롯 (1·2=무기, 3=맨손)
    EquipItem_Weapon CurrentWeapon  { get; } // 현재 손에 들고 있는 무기

    MeshComponent MeshComp          { get; } // 메쉬 소켓 참조

    void OnWeaponSlot(int slot);     // 무기 슬롯 선택 시 호출
    void WeaponSlotUpdate();         // 무기 슬롯 변경 시 호출
}
