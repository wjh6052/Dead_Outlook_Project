using UnityEngine;
using UnityEngine.EventSystems;

// 이 컴포넌트가 붙은 패널을 드래그하면 부모 창 전체가 이동
public class UI_DragWindow : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform WindowRt; // 이동할 부모 창의 RectTransform
    private Vector2       DragOffset; // 드래그 시작 시 마우스와 창 위치 간 오프셋

    public void Info(RectTransform windowRt)
    {
        WindowRt = windowRt;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragOffset = (Vector2)WindowRt.position - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPos = eventData.position + DragOffset;

        // 창 크기를 고려해 화면 밖으로 나가지 않도록 클램프
        float halfW = WindowRt.rect.width  * 0.5f;
        float halfH = WindowRt.rect.height * 0.5f;

        newPos.x = Mathf.Clamp(newPos.x, halfW, Screen.width  - halfW);
        newPos.y = Mathf.Clamp(newPos.y, halfH, Screen.height - halfH);

        WindowRt.position = newPos;
    }
}
