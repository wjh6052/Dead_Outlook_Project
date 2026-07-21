using Unity.VisualScripting;
using UnityEngine;

public class UI_EquipSlot : UI_ItemGrid
{
    [Header("장착 가능한 아이템 타입")]
    [SerializeField] private EItemType AcceptType; // 장착 가능한 아이템 타입

    public UI_ItemIcon  EquippedIcon { get; private set; } // 현재 장착 슬롯의 아이콘
    private UI_ItemSlot         Slot;         // 고정 슬롯
    private System.Action<Item> EquippedItem; // 장착/해제 시 외부 데이터 갱신 콜백

    public void Init(System.Action<Item> equippedItem = null)
    {
        EquippedItem = equippedItem;

        Slot = transform.Find("EquipSlot").GetComponent<UI_ItemSlot>();
        Slot.Init(this, 0, 0);

        Slots      = new UI_ItemSlot[1, 1];
        OccupiedBy = new UI_ItemIcon[1, 1];
        Slots[0, 0] = Slot;
    }

    // 슬롯에 장착 가능한 아이템인지 확인 — 타입 불일치 시 false, 타입별 세부 조건 추가 가능
    private bool CanAccept(Item item)
    {
        if (item.ItemType != AcceptType) return false;

        switch (AcceptType)
        {
            case EItemType.Bag: return item is Item_Bag;
            default:            return true;
        }
    }

    #region UI_ItemIcon에서 호출
    public override void HighlightSlots(int x, int y, Item item)
    {
        Slot.SetHighlight(EquippedIcon == null && CanAccept(item) ? ESlotHighlight.Valid : ESlotHighlight.Invalid);
    }

    public override bool TryDrop(Item item, int x, int y, UI_ItemGrid source, UI_ItemIcon sourceIcon)
    {
        // 슬롯이 비어있으면 장착
        if (EquippedIcon == null)
        {
            if (!CanAccept(item)) return false;

            source.RemoveItem(sourceIcon);
            EquippedItem?.Invoke(item);

            sourceIcon.MoveTo(this, Slot.transform, this);

            EquippedIcon     = sourceIcon;
            OccupiedBy[0, 0] = sourceIcon;

            return true;
        }

        return false;
    }

    public override void RemoveItem(UI_ItemIcon icon)
    {
        EquippedIcon     = null;
        OccupiedBy[0, 0] = null;
        EquippedItem?.Invoke(null);
    }
    #endregion
}
