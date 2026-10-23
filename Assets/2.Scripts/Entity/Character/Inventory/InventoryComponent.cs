using UnityEngine;

public class InventoryComponent : MonoBehaviour
{

    [Tooltip("주머니 인벤토리 데이터 (고정)")]
    public InventoryData  PocketData;  // 주머니 인벤토리 데이터 (고정)
    [Tooltip("현재 장착된 조끼 아이템")]
    public Item_Container VestItem;    // 현재 장착된 조끼 아이템
    [Tooltip("현재 장착된 가방 아이템")]
    public Item_Container BagItem;     // 현재 장착된 가방 아이템
    [Tooltip("1번 슬롯에 장착된 무기 아이템")]
    public Item           Weapon_1_Item; // 현재 장착된 무기 1
    [Tooltip("2번 슬롯에 장착된 무기 아이템")]
    public Item           Weapon_2_Item; // 현재 장착된 무기 2

    [Tooltip("WeaponSocket_1에 생성된 무기 오브젝트")]
    public EquipItem_Weapon Weapon_1; // 현재 WeaponSocket_1에 장착된 무기 오브젝트
    [Tooltip("WeaponSocket_2에 생성된 무기 오브젝트")]
    public EquipItem_Weapon Weapon_2; // 현재 WeaponSocket_2에 장착된 무기 오브젝트

    [Tooltip("BagSocket에 생성된 가방 오브젝트")]
    public EquipItem_Container_Bag BagObject; // 현재 BagSocket에 장착된 가방 오브젝트

    [Tooltip("VestSocket에 생성된 조끼 오브젝트")]
    public EquipItem_Container_Vest VestObject; // 현재 VestSocket에 장착된 조끼 오브젝트

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
        foreach (Transform child in InventoryOwner.MeshComp.BagSocket) Destroy(child.gameObject);
        foreach (Transform child in InventoryOwner.MeshComp.VestSocket) Destroy(child.gameObject);

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
            SetVest(null); // 조끼 해제 — 몸의 조끼 오브젝트도 제거
        }

        // 가방 초기화
        if (BagItem != null)
        {
            BagItem.ContainerInventory.Items.Clear();
            BagItem.ContainerInventory.OnChanged?.Invoke();
            SetBag(null); // 가방 해제 — 등의 가방 오브젝트도 제거
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
                // 아이템 프리팹을 WeaponSocket_1에 소환 후 EquipItem_Weapon 컴포넌트 초기화
                Weapon_1 = Instantiate(item.Prefab, InventoryOwner.MeshComp.WeaponSocket_1).GetComponent<EquipItem_Weapon>();
                Weapon_1.InitEquip(item, gameObject);
                Weapon_1.AttachToHolster(InventoryOwner.MeshComp.WeaponSocket_1); // 홀스터 포인트 기준으로 위치 보정
            }
        }
        else
        {
            Weapon_2_Item = item;
            if (Weapon_2 != null) 
                Destroy(Weapon_2.gameObject); // 기존 무기 오브젝트 제거

            if (item != null)
            {
                // 아이템 프리팹을 WeaponSocket_2에 소환 후 EquipItem_Weapon 컴포넌트 초기화
                Weapon_2 = Instantiate(item.Prefab, InventoryOwner.MeshComp.WeaponSocket_2).GetComponent<EquipItem_Weapon>();
                Weapon_2.InitEquip(item, gameObject);
                Weapon_2.AttachToHolster(InventoryOwner.MeshComp.WeaponSocket_2); // 홀스터 포인트 기준으로 위치 보정
            }
        }

        InventoryOwner.WeaponSlotUpdate();
    }

    // 가방 장착/해제 — item이 null이면 해제 / 장착하면 가방 오브젝트를 BagSocket에 표시
    public void SetBag(Item_Container item)
    {
        BagItem = item;
        if (BagObject != null)
            Destroy(BagObject.gameObject); // 기존 가방 오브젝트 제거
        BagObject = null;

        if (item == null) return;

        // 아이템 프리팹을 BagSocket에 소환
        GameObject bagObject = Instantiate(item.Prefab, InventoryOwner.MeshComp.BagSocket);
        BagObject = bagObject.GetComponent<EquipItem_Container_Bag>();
        if (BagObject == null)
        {
            // 가방 컴포넌트가 없는 프리팹 — 데이터 장착은 유지하고 등에 보이는 오브젝트만 생략
            Debug.LogWarning($"{bagObject.name}에 EquipItem_Container_Bag이 없습니다 ({item.ID})");
            Destroy(bagObject);
            return;
        }

        // 등에 멘 가방은 바닥 아이템이 아니므로 줍기 · 물리 · 충돌 비활성화 (EquipItem)
        BagObject.InitEquip(item, gameObject);
        BagObject.AttachToHolster(InventoryOwner.MeshComp.BagSocket); // 홀스터 포인트 기준으로 위치 보정
    }

    // 조끼 장착/해제 — item이 null이면 해제 / 장착하면 조끼 오브젝트를 VestSocket에 표시
    public void SetVest(Item_Container item)
    {
        VestItem = item;
        if (VestObject != null)
            Destroy(VestObject.gameObject); // 기존 조끼 오브젝트 제거
        VestObject = null;

        if (item == null) return;

        // 아이템 프리팹을 VestSocket에 소환
        GameObject vestObject = Instantiate(item.Prefab, InventoryOwner.MeshComp.VestSocket);
        VestObject = vestObject.GetComponent<EquipItem_Container_Vest>();
        if (VestObject == null)
        {
            // 조끼 컴포넌트가 없는 프리팹 — 데이터 장착은 유지하고 몸에 보이는 오브젝트만 생략
            Debug.LogWarning($"{vestObject.name}에 EquipItem_Container_Vest가 없습니다 ({item.ID})");
            Destroy(vestObject);
            return;
        }

        // 입은 조끼는 바닥 아이템이 아니므로 줍기 · 물리 · 충돌 비활성화 (EquipItem)
        VestObject.InitEquip(item, gameObject);
        VestObject.AttachToHolster(InventoryOwner.MeshComp.VestSocket); // 홀스터 포인트 기준으로 위치 보정
    }


}
