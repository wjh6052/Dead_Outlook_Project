using UnityEngine;
using UnityEngine.EventSystems;

// 창 이동 핸들 — 드래그 시 부모 창을 이동
public class UI_WindowDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    private RectTransform WindowRT;
    private Vector2       DragOffset;

    public void Init(RectTransform windowRT)
    {
        WindowRT = windowRT;
        Debug.Log("성공");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            WindowRT.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        DragOffset = WindowRT.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            WindowRT.parent as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        WindowRT.anchoredPosition = localPoint + DragOffset;
    }
}
