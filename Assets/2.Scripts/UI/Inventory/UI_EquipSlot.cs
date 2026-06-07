using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_EquipSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool         IsBeltSlot;      // 벨트 전용 슬롯 여부
    [SerializeField] public  UI_Inventory LinkedInventory; // 이 슬롯에 연결된 인벤토리
    public Image SlotImage; // 슬롯 배경 이미지

    private InventoryComponent Owner;        // 이 슬롯의 소유자 인벤토리 컴포넌트
    private Item_Bag           EquippedItem; // 현재 장착된 가방 아이템
    private UI_Item_Icon       EquippedIcon; // 장착 아이템 아이콘

    private Color HighlightColor = new Color(0.8f, 0.8f, 0f, 1f); // 노란색
    private Color InvalidColor   = new Color(0.8f, 0f,   0f, 1f); // 빨간색
    private Color OriginalColor;

    private void Awake()
    {
        OriginalColor = SlotImage.color;

        LinkedInventory.gameObject.SetActive(false);
    }


    // 소유자 설정
    public void Init(InventoryComponent owner)
    {
        Owner = owner;
    }

    // IDropHandler — 드래그한 아이콘을 슬롯에 놓았을 때 호출
    public void OnDrop(PointerEventData eventData)
    {
        UI_Item_Icon icon = UI_Item_Icon.BeingDraggedIcon;
        if (icon == null) return;

        // 이미 장착된 아이템이 있으면 무시
        if (EquippedIcon != null) return;

        // Item_Bag가 아니거나 슬롯 타입이 맞지 않으면 무시
        if (icon.ItemData is not Item_Bag bag) return;
        if (bag.IsBelt != IsBeltSlot) return;

        

        Equip(bag, icon);
    }

    // IPointerEnterHandler — 드래그 중인 아이콘이 슬롯 위에 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UI_Item_Icon.BeingDraggedIcon == null) return;
        if (EquippedIcon != null) return;

        bool canEquip = UI_Item_Icon.BeingDraggedIcon.ItemData is Item_Bag bag
                     && bag.IsBelt == IsBeltSlot;

        SlotImage.color = canEquip ? HighlightColor : InvalidColor;
    }

    // IPointerExitHandler — 드래그 중인 아이콘이 슬롯에서 벗어났을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        SlotImage.color = OriginalColor;
    }

    // 가방 장착 — 아이콘을 슬롯 자식으로 이동 후 연결된 인벤토리 초기화
    private void Equip(Item_Bag bag, UI_Item_Icon icon)
    {
        EquippedItem = bag;
        EquippedIcon = icon;

        // Owner에 장착 정보 저장
        if (Owner != null)
        {
            if (IsBeltSlot) Owner.BeltItem = bag;
            else            Owner.BagItem  = bag;
        }

        // 기존 인벤토리에서 제거 (장착 슬롯은 InventoryData에 속하지 않음)
        icon.TransferTo(null);

        // 아이콘을 이 슬롯의 자식으로 이동 후 중앙 정렬
        icon.OwnerEquipSlot = this;
        icon.transform.SetParent(SlotImage.transform);
        RectTransform iconRt   = icon.GetComponent<RectTransform>();
        iconRt.anchorMin        = new Vector2(0.5f, 0.5f);
        iconRt.anchorMax        = new Vector2(0.5f, 0.5f);
        iconRt.pivot            = new Vector2(0.5f, 0.5f);
        iconRt.anchoredPosition = Vector2.zero;

        // 아이콘 크기를 2x2로 조정
        icon.SetDisplaySize(true);

        // 연결된 인벤토리를 가방 데이터로 초기화
        LinkedInventory.gameObject.SetActive(true);
        LinkedInventory.UpdateInventory(EquippedItem.BagInventory);
        Mgr_UI.Inst.RegisterOpenInventory(EquippedItem.BagInventory, LinkedInventory);

        ScrollRect scrollRect = LinkedInventory.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            Canvas.ForceUpdateCanvases();
        }
    }

    // 장착 해제 — 슬롯 비우기
    public void Unequip()
    {
        if (EquippedIcon != null)
        {
            EquippedIcon.SetDisplaySize(false);
            EquippedIcon.OwnerEquipSlot = null;
        }
        if (EquippedItem != null) Mgr_UI.Inst.UnregisterOpenInventory(EquippedItem.BagInventory);

        EquippedItem = null;
        EquippedIcon = null;

        // Owner에서 장착 정보 제거
        if (Owner != null)
        {
            if (IsBeltSlot) Owner.BeltItem = null;
            else            Owner.BagItem  = null;
        }
        LinkedInventory.gameObject.SetActive(false);
    }
}
