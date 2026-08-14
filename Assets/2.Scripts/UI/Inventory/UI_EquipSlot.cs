using Unity.VisualScripting;
using UnityEngine;

public enum EEquipSlotType 
{ 
    Vest, 
    Bag, 
    Weapon1, 
    Weapon2
}

public class UI_EquipSlot : UI_ItemGrid
{
    [Header("장착 슬롯 설정")]
    [SerializeField] private EEquipSlotType SlotType;

    public UI_ItemIcon  EquippedIcon { get; private set; } // 현재 장착 슬롯의 아이콘
    private UI_ItemSlot         Slot;         // 고정 슬롯
    private System.Action<Item> EquippedItem; // 장착/해제 시 외부 데이터 갱신 콜백

    public void Setup()
    {
        // 기존 아이콘 오브젝트 제거
        if (EquippedIcon != null)
        {
            Destroy(EquippedIcon.gameObject);
            EquippedIcon = null;
        }

        Slot        = transform.Find("EquipSlot").GetComponent<UI_ItemSlot>();
        Slot.Init(this, 0, 0);
        Slots       = new UI_ItemSlot[1, 1];
        OccupiedBy  = new UI_ItemIcon[1, 1];
        Slots[0, 0] = Slot;
    }

    // 장착/해제 시 호출할 외부 콜백 등록
    public void Init(System.Action<Item> equippedItem = null)
    {
        EquippedItem = equippedItem;
    }

    // 코드에서 직접 장착 — MainCanvas 아래에 생성 후 MoveTo로 슬롯으로 이동
    public void Equip(Item item)
    {
        if (EquippedIcon != null) return;
        UI_ItemIcon icon = Instantiate(Mgr_UI.Inst.IconPrefab, Mgr_UI.Inst.MainCanvas.transform);
        icon.Init(item, null);
        icon.MoveTo(this, Slot.transform, this);
        EquippedIcon     = icon;
        OccupiedBy[0, 0] = icon;
        EquippedItem?.Invoke(item);
    }




    // SlotType 기준으로 아이템 장착 가능 여부 반환
    private bool CanAccept(Item item)
    {
        switch (SlotType)
        {
            case EEquipSlotType.Weapon1:
            case EEquipSlotType.Weapon2:
                return item.ItemType == EItemType.WeaponMelee || item.ItemType == EItemType.WeaponRanged;
            case EEquipSlotType.Vest:
                return item is Item_Container vest && vest.ContainerType == EContainerType.Vest;
            case EEquipSlotType.Bag:
                return item is Item_Container bag && bag.ContainerType == EContainerType.Bag;
            default:
                return false;
        }
    }

    #region UI_ItemIcon에서 호출
    // 드래그 중 슬롯 위에 올렸을 때 호출 — 장착 가능 여부에 따라 초록/빨강 하이라이트
    public override void HighlightSlots(int x, int y, Item item)
    {
        if (EquippedIcon != null) { Slot.SetHighlight(ESlotHighlight.Invalid); return; }
        Slot.SetHighlight(CanAccept(item) ? ESlotHighlight.Valid : ESlotHighlight.Invalid);
    }

    // 드롭 시 호출 — 슬롯이 비어있고 타입 검사 통과 시 원본에서 제거 후 이 슬롯에 장착
    public override bool TryDrop(Item item, int x, int y, UI_ItemGrid source, UI_ItemIcon sourceIcon)
    {
        if (EquippedIcon != null || !CanAccept(item)) return false;

        source.RemoveItem(sourceIcon);
        EquippedItem?.Invoke(item);
        sourceIcon.MoveTo(this, Slot.transform, this);
        EquippedIcon     = sourceIcon;
        OccupiedBy[0, 0] = sourceIcon;
        return true;
    }

    // 아이템이 슬롯에서 빠져나갈 때 호출 — EquippedIcon 초기화 및 장착 해제 콜백 전달
    public override void RemoveItem(UI_ItemIcon icon)
    {
        EquippedIcon     = null;
        OccupiedBy[0, 0] = null;
        EquippedItem?.Invoke(null);
    }
    #endregion
}
