using UnityEngine;

public class InventoryComponent : MonoBehaviour
{

    public InventoryData  PocketData;  // 주머니 인벤토리 데이터 (고정)
    public Item_Container VestItem;    // 현재 장착된 조끼 아이템
    public Item_Container BagItem;     // 현재 장착된 가방 아이템
    public Item           Weapon_1_Item; // 현재 장착된 무기 1
    public Item           Weapon_2_Item; // 현재 장착된 무기 2

    public Weapon Weapon_1; // 현재 WeaponSocket_1에 장착된 무기 오브젝트
    public Weapon Weapon_2; // 현재 WeaponSocket_2에 장착된 무기 오브젝트

    private IInventory InventoryOwner; // 같은 오브젝트의 IInventory 구현체

    private void Awake()
    {
        InventoryOwner = GetComponent<IInventory>();
    }


    private System.Collections.IEnumerator Start()
    {
        // 소켓에 오브젝트 제거 — MeshComp는 Character_Player.Awake에서 세팅되므로 Start에서 접근
        foreach (Transform child in InventoryOwner.MeshComp.HandSocket_R) Destroy(child.gameObject);
        foreach (Transform child in InventoryOwner.MeshComp.WeaponSocket_1) Destroy(child.gameObject);
        foreach (Transform child in InventoryOwner.MeshComp.WeaponSocket_2) Destroy(child.gameObject);

        yield return null;
    }


    // 장착 슬롯 자동 장착 시도 → Pocket → Vest → Bag 순서로 아이템 추가
    public bool TryAddItem(Item item)
    {
        if (Mgr_UI.Inst.UI_Inventory_Player.TryEquip(item)) return true;
        if (PocketData.TryAddItem(item)) return true;
        if (VestItem?.ContainerInventory.TryAddItem(item) == true) return true;
        if (BagItem?.ContainerInventory.TryAddItem(item) == true) return true;
        return false;
    }


    // 모든 인벤토리 초기화 — 사망 시 아이템 손실 처리
    public void ClearInventory()
    {
        // 주머니 초기화
        PocketData.Items.Clear();
        PocketData.OnChanged?.Invoke();

        // 조끼 초기화
        if (VestItem != null)
        {
            VestItem.ContainerInventory.Items.Clear();
            VestItem.ContainerInventory.OnChanged?.Invoke();
            VestItem = null;
        }

        // 가방 초기화
        if (BagItem != null)
        {
            BagItem.ContainerInventory.Items.Clear();
            BagItem.ContainerInventory.OnChanged?.Invoke();
            BagItem = null;
        }

        // 무기 해제
        SetWeapon(true,  null);
        SetWeapon(false, null);

        // 장착 슬롯 UI 초기화 — 아이콘 제거 + 콜백 재등록
        Mgr_UI.Inst.UI_Inventory_Player.Setup();
    }

    // 무기 장착/해제 — isFirst: true=1번 슬롯, false=2번 슬롯 / item이 null이면 해제
    public void SetWeapon(bool isFirst, Item item)
    {
        if (isFirst)
        {
            Weapon_1_Item = item;
            if (Weapon_1 != null) 
                Destroy(Weapon_1.gameObject); // 기존 무기 오브젝트 제거

            if (item != null)
            {
                // 아이템 프리팹을 WeaponSocket_1에 소환 후 Weapon 컴포넌트 초기화
                Weapon_1 = Instantiate(item.Prefab, InventoryOwner.MeshComp.WeaponSocket_1).GetComponent<Weapon>();
                Weapon_1.Init(item as Item_Weapon, gameObject);
            }
        }
        else
        {
            Weapon_2_Item = item;
            if (Weapon_2 != null) 
                Destroy(Weapon_2.gameObject); // 기존 무기 오브젝트 제거

            if (item != null)
            {
                // 아이템 프리팹을 WeaponSocket_2에 소환 후 Weapon 컴포넌트 초기화
                Weapon_2 = Instantiate(item.Prefab, InventoryOwner.MeshComp.WeaponSocket_2).GetComponent<Weapon>();
                Weapon_2.Init(item as Item_Weapon, gameObject);
            }
        }

        InventoryOwner.WeaponSlotUpdate();
    }


}
