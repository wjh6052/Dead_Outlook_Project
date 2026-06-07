using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Item_Slot : MonoBehaviour, IDropHandler
{
    public int               GridX; // 그리드 내 X 좌표
    public int               GridY; // 그리드 내 Y 좌표
    public UI_Inventory_Grid Grid { get; private set; } // 소속 그리드 참조

    private Image SlotImage;

    [SerializeField] private Color NormalColor  = new Color(100/255f, 100/255f, 100/255f, 1f); // 기본 색상
    [SerializeField] private Color ValidColor   = new Color(0f, 0.8f, 0f, 1f);                 // 배치 가능 (초록)
    [SerializeField] private Color InvalidColor = new Color(0.8f, 0f, 0f, 1f);                 // 배치 불가 (빨강)
    [SerializeField] private Color PartialColor = new Color(0.8f, 0.8f, 0f, 1f);               // 부분 스택 가능 (노랑)

    // UI_Inventory_Grid에서 슬롯 생성 시 호출 — 좌표와 그리드 참조 설정
    public void Init(int x, int y, UI_Inventory_Grid grid)
    {
        SlotImage = GetComponent<Image>();
        GridX     = x;
        GridY     = y;
        Grid      = grid;
        SlotImage.color = NormalColor;
    }

    public void SetHighlight() { SlotImage.color = ValidColor;   }
    public void SetInvalid()   { SlotImage.color = InvalidColor; }
    public void SetPartial()   { SlotImage.color = PartialColor; }
    public void ResetColor()   { SlotImage.color = NormalColor;  }

    // 이 슬롯 위에 드래그된 아이콘을 놓았을 때 호출
    public void OnDrop(PointerEventData eventData)
    {
        UI_Item_Icon icon = UI_Item_Icon.BeingDraggedIcon?.GetComponent<UI_Item_Icon>();
        if (icon == null) return;

        if (Grid.CanPlace(GridX, GridY, icon.SizeX, icon.SizeY))
        {
            icon.TransferTo(Grid.Inventory?.Data);
            Grid.Place(icon, GridX, GridY);
        }
    }
}
