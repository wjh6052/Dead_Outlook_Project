using UnityEngine;
using UnityEngine.UI;

public enum ESlotHighlight
{
    Default, // 기본 상태 (드래그 없음)
    Valid,   // 완전 배치 가능 (빈 슬롯에 딱 맞게 놓을 수 있음)
    Invalid, // 배치 불가 (슬롯이 막혀 있거나 범위를 벗어남)
    Partial  // 스택 가능 (같은 아이템이 있고 수량을 합칠 수 있음)
}

public class UI_ItemSlot : MonoBehaviour
{
    public UI_InventoryGrid Inventory; // 소속 인벤토리
    public int          GridX;     // 그리드 X 좌표
    public int          GridY;     // 그리드 Y 좌표

    private Image SlotImage; // 슬롯 배경 이미지

    // UI_InventoryGrid/Update_Slots에서 슬롯 생성 시 호출
    public void Init(UI_InventoryGrid inventory, int x, int y)
    {
        Inventory  = inventory;
        GridX      = x;
        GridY      = y;
        SlotImage  = GetComponent<Image>();
    }

    public void SetHighlight(ESlotHighlight highlight)
        => SlotImage.color = Mgr_UI.Inst.GetHighlightColor(highlight);
}
